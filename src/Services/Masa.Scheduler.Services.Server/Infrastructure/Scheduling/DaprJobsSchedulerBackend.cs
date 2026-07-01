// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public class DaprJobsSchedulerBackend : ISchedulerBackend
{
    private readonly DaprJobsClient _daprJobsClient;
    private readonly IOptions<SchedulerBackendOptions> _options;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;
    private readonly ILogger<DaprJobsSchedulerBackend> _logger;

    public DaprJobsSchedulerBackend(DaprJobsClient daprJobsClient, IOptions<SchedulerBackendOptions> options, IMultiEnvironmentContext multiEnvironmentContext, ILogger<DaprJobsSchedulerBackend> logger)
    {
        _daprJobsClient = daprJobsClient;
        _options = options;
        _multiEnvironmentContext = multiEnvironmentContext;
        _logger = logger;
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task RegisterCronJob(string environment, Guid jobId, string cron)
    {
        var name = DaprJobsNameHelper.BuildCronName(environment, jobId);
        var payload = new DaprJobPayload
        {
            Type = DaprJobNameType.Cron,
            JobId = jobId,
            Environment = environment,
            CronExpression = cron,
            CronTimeZone = _options.Value.DaprJobs.CronTimeZone
        };

        return ScheduleCronJobAsync(name, cron, payload);
    }

    public Task RemoveCronJob(Guid jobId)
    {
        var name = DaprJobsNameHelper.BuildCronName(_multiEnvironmentContext.CurrentEnvironment, jobId);
        return DeleteJobAsync(name, throwOnMissing: true);
    }

    public async Task AddDelayTask(string environment, Guid taskId, Guid jobId, TimeSpan delay)
    {
        var name = DaprJobsNameHelper.BuildRetryName(environment, jobId, taskId);
        var payload = new DaprJobPayload
        {
            Type = DaprJobNameType.Retry,
            JobId = jobId,
            TaskId = taskId,
            Environment = environment
        };

        if (_options.Value.DaprJobs.Overwrite)
        {
            await TryDeleteJobAsync(name);
        }

        var dueTime = DateTimeOffset.UtcNow.Add(delay);
        var result = await ScheduleJobAsync(name, DaprJobSchedule.FromDateTime(dueTime), payload, startingFrom: null, ttl: null);
        if (!result.Success)
        {
            _logger.LogWarning("DaprJobs delay schedule failed. Name: {Name}. Error: {Error}", name, result.Error);
        }
    }

    public Task RemoveDelayTask(Guid taskId, Guid jobId)
    {
        var name = DaprJobsNameHelper.BuildRetryName(_multiEnvironmentContext.CurrentEnvironment, jobId, taskId);
        return DeleteJobAsync(name, throwOnMissing: true);
    }

    public Task<List<DateTimeOffset>> GetCronExecuteTimeByTimeRange(string cron, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        return DaprJobsTimeCalculator.GetCronExecuteTimeByTimeRange(cron, startTime, endTime);
    }

    private async Task DeleteJobAsync(string name, bool throwOnMissing)
    {
        await TryDeleteJobAsync(name, throwOnMissing);
    }

    private async Task TryDeleteJobAsync(string name, bool throwOnMissing = false)
    {
        try
        {
            await _daprJobsClient.DeleteJobAsync(name);
        }
        catch (Exception ex)
        {
            if (!throwOnMissing && DaprJobsExceptionHelper.IsNotFound(ex))
            {
                _logger.LogInformation("Delete Dapr job ignored because job was not found. Name: {Name}", name);
                return;
            }

            _logger.LogError(ex, "Delete Dapr job failed. Name: {Name}", name);
            throw;
        }
    }

    private async Task ScheduleCronJobAsync(string name, string cron, DaprJobPayload payload)
    {
        var candidates = BuildCronCandidates(cron, _options.Value.DaprJobs.CronTimeZone);
        var activationWindow = BuildCronActivationWindow(cron, _options.Value.DaprJobs.CronTimeZone);
        if (candidates.Count == 0)
        {
            throw new UserFriendlyException("CronExpression is empty");
        }

        if (_options.Value.DaprJobs.Overwrite)
        {
            await TryDeleteJobAsync(name);
        }

        string? lastError = null;
        foreach (var candidate in candidates)
        {
            var result = await TryScheduleCronViaSdkAsync(name, candidate, payload, activationWindow.StartingFrom, activationWindow.Ttl);
            if (result.Success)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                lastError = result.Error;
            }
        }

        _logger.LogWarning("DaprJobs schedule candidates failed. Cron: {Cron}. Candidates: {Candidates}", cron, string.Join(" | ", candidates));
        var message = string.IsNullOrWhiteSpace(lastError) ? "DaprJobs schedule failed" : $"DaprJobs schedule failed: {lastError}";
        throw new UserFriendlyException(message);
    }

    private static List<string> BuildCronCandidates(string cron, string? cronTimeZone)
    {
        if (string.IsNullOrWhiteSpace(cron))
        {
            return new List<string>();
        }

        var cronExpression = ExtractCronBodyAndResolveTimeZone(cron, cronTimeZone, out var resolvedCronTimeZone);
        var normalized = cronExpression.Trim().Replace('?', '*');
        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var candidates = new List<string>();

        AddCronCandidates(candidates, parts);

        if (parts.Any(part => part.StartsWith("0/", StringComparison.Ordinal)))
        {
            var variant = parts
                .Select(part => part.StartsWith("0/", StringComparison.Ordinal)
                    ? $"*/{part[2..]}"
                    : part)
                .ToArray();
            AddCronCandidates(candidates, variant);
        }

        var normalizedCandidates = candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (string.IsNullOrWhiteSpace(resolvedCronTimeZone))
        {
            return normalizedCandidates;
        }

        var timeZonePrefix = $"CRON_TZ={resolvedCronTimeZone.Trim()} ";
        var withTimeZoneCandidates = normalizedCandidates
            .Select(candidate => $"{timeZonePrefix}{candidate}");

        return withTimeZoneCandidates
            .Concat(normalizedCandidates)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string ExtractCronBodyAndResolveTimeZone(string cron, string? defaultCronTimeZone, out string? resolvedCronTimeZone)
    {
        var tokens = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        resolvedCronTimeZone = defaultCronTimeZone;

        if (tokens.Length > 1 && (tokens[0].StartsWith("CRON_TZ=", StringComparison.OrdinalIgnoreCase) || tokens[0].StartsWith("TZ=", StringComparison.OrdinalIgnoreCase)))
        {
            var separatorIndex = tokens[0].IndexOf('=');
            if (separatorIndex >= 0 && separatorIndex + 1 < tokens[0].Length)
            {
                var inlineTimeZone = tokens[0][(separatorIndex + 1)..];
                if (!string.IsNullOrWhiteSpace(inlineTimeZone))
                {
                    resolvedCronTimeZone = inlineTimeZone;
                }
            }

            return string.Join(' ', tokens.Skip(1));
        }

        return string.Join(' ', tokens);
    }

    private static CronActivationWindow BuildCronActivationWindow(string cron, string? cronTimeZone)
    {
        var cronExpression = ExtractCronBodyAndResolveTimeZone(cron, cronTimeZone, out var resolvedCronTimeZone);
        var parts = cronExpression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 7)
        {
            return CronActivationWindow.Empty;
        }

        var yearToken = parts[6];
        if (string.IsNullOrWhiteSpace(yearToken) || yearToken == "*" || yearToken == "?")
        {
            return CronActivationWindow.Empty;
        }

        var yearRange = ParseYearRange(yearToken);
        if (yearRange == null)
        {
            return CronActivationWindow.Empty;
        }

        var timeZone = ResolveCronTimeZone(resolvedCronTimeZone);
        var dueTime = CreateOffsetDateTime(timeZone, yearRange.Value.Min, 1, 1);
        DateTimeOffset? ttl = null;
        if (yearRange.Value.Max < 9999)
        {
            ttl = CreateOffsetDateTime(timeZone, yearRange.Value.Max + 1, 1, 1);
        }

        return new CronActivationWindow(dueTime, ttl);
    }

    private static (int Min, int Max)? ParseYearRange(string yearToken)
    {
        if (yearToken.Contains('/'))
        {
            throw new UserFriendlyException($"DaprJobs does not support cron year step expression: {yearToken}");
        }

        var segments = yearToken.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return null;
        }

        var intervals = new List<(int Start, int End)>();
        foreach (var segment in segments)
        {
            if (int.TryParse(segment, out var singleYear))
            {
                intervals.Add((singleYear, singleYear));
                continue;
            }

            var rangeParts = segment.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (rangeParts.Length == 2
                && int.TryParse(rangeParts[0], out var startYear)
                && int.TryParse(rangeParts[1], out var endYear)
                && startYear <= endYear)
            {
                intervals.Add((startYear, endYear));
                continue;
            }

            throw new UserFriendlyException($"DaprJobs does not support cron year expression: {yearToken}");
        }

        if (intervals.Count == 0)
        {
            return null;
        }

        var orderedIntervals = intervals.OrderBy(interval => interval.Start).ToList();
        var minYear = orderedIntervals[0].Start;
        var maxYear = orderedIntervals[0].End;
        for (var index = 1; index < orderedIntervals.Count; index++)
        {
            var current = orderedIntervals[index];
            if (current.Start <= maxYear + 1)
            {
                maxYear = Math.Max(maxYear, current.End);
                continue;
            }

            throw new UserFriendlyException($"DaprJobs does not support discontinuous cron year expression: {yearToken}");
        }

        return (minYear, maxYear);
    }

    private static DateTimeOffset CreateOffsetDateTime(TimeZoneInfo timeZone, int year, int month, int day)
    {
        var localDateTime = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
        var utcOffset = timeZone.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, utcOffset);
    }

    private static TimeZoneInfo ResolveCronTimeZone(string? cronTimeZone)
    {
        if (!string.IsNullOrWhiteSpace(cronTimeZone))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(cronTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.Local;
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.Local;
            }
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }

    private static void AddCronCandidates(List<string> candidates, string[] parts)
    {
        if (parts.Length == 6)
        {
            var normalized = NormalizeDayOfWeek(parts, 5);
            candidates.Add(string.Join(' ', normalized));
        }
        else if (parts.Length == 7)
        {
            // Dapr Jobs does not support year field in cron expression.
            // Convert Quartz 7-field cron (sec min hour day month dayOfWeek year)
            // to 6-field cron by dropping the trailing year segment.
            var sixFieldCronParts = parts.Take(6).ToArray();
            var normalized = NormalizeDayOfWeek(sixFieldCronParts, 5);
            candidates.Add(string.Join(' ', normalized));
        }
        else if (parts.Length == 5)
        {
            // Keep backwards compatibility for legacy 5-field inputs by prepending seconds.
            var normalized = NormalizeDayOfWeek(parts, 4);
            candidates.Add($"0 {string.Join(' ', normalized)}");
        }
    }

    private static string[] NormalizeDayOfWeek(string[] parts, int index)
    {
        if (index < 0 || index >= parts.Length)
        {
            return parts;
        }

        var value = parts[index];
        if (string.IsNullOrWhiteSpace(value) || value == "*")
        {
            return parts;
        }

        var normalizedValue = NormalizeQuartzDayOfWeekToken(value);
        var copy = parts.ToArray();
        copy[index] = normalizedValue;
        return copy;
    }

    private static string NormalizeQuartzDayOfWeekToken(string value)
    {
        // Dapr jobs cron follows standard 0-6 (Sun-Sat),
        // while Quartz numeric day-of-week uses 1-7 (Sun-Sat).
        if (string.Equals(value, "?", StringComparison.Ordinal))
        {
            return "*";
        }

        if (value.Contains('#', StringComparison.Ordinal) || value.Contains('L', StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException($"DaprJobs does not support Quartz day-of-week syntax: {value}");
        }

        var normalizedSegments = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeQuartzDayOfWeekSegment)
            .Distinct(StringComparer.Ordinal);

        return string.Join(',', normalizedSegments);
    }

    private static string NormalizeQuartzDayOfWeekSegment(string segment)
    {
        var stepParts = segment.Split('/', StringSplitOptions.TrimEntries);
        if (stepParts.Length > 2)
        {
            throw new UserFriendlyException($"DaprJobs does not support Quartz day-of-week syntax: {segment}");
        }

        var normalizedBase = NormalizeQuartzDayOfWeekBase(stepParts[0]);
        if (stepParts.Length == 1)
        {
            return normalizedBase;
        }

        if (!int.TryParse(stepParts[1], out var step) || step <= 0)
        {
            throw new UserFriendlyException($"DaprJobs does not support Quartz day-of-week step syntax: {segment}");
        }

        return $"{normalizedBase}/{step}";
    }

    private static string NormalizeQuartzDayOfWeekBase(string value)
    {
        if (string.Equals(value, "*", StringComparison.Ordinal) || string.Equals(value, "?", StringComparison.Ordinal))
        {
            return "*";
        }

        if (int.TryParse(value, out var single))
        {
            return MapQuartzDayOfWeekToDapr(single).ToString();
        }

        var rangeParts = value.Split('-', StringSplitOptions.TrimEntries);
        if (rangeParts.Length == 2
            && int.TryParse(rangeParts[0], out var rangeStart)
            && int.TryParse(rangeParts[1], out var rangeEnd))
        {
            var mappedStart = MapQuartzDayOfWeekToDapr(rangeStart);
            var mappedEnd = MapQuartzDayOfWeekToDapr(rangeEnd);
            if (mappedStart <= mappedEnd)
            {
                return $"{mappedStart}-{mappedEnd}";
            }

            // Quartz allows wrapped ranges like 6-2.
            // Expand to a list so Dapr can consume a standard 0-6 set.
            var wrappedRange = Enumerable.Range(mappedStart, 7 - mappedStart)
                .Concat(Enumerable.Range(0, mappedEnd + 1));
            return string.Join(',', wrappedRange);
        }

        // Preserve named values/ranges like MON-FRI.
        return value;
    }

    private static int MapQuartzDayOfWeekToDapr(int value)
    {
        // Quartz numeric day-of-week: 1-7 => SUN-SAT.
        // Dapr/systemd day-of-week: 0-6 => SUN-SAT.
        if (value is >= 1 and <= 7)
        {
            return value - 1;
        }

        throw new UserFriendlyException($"DaprJobs does not support Quartz day-of-week value: {value}");
    }

    private async Task<ScheduleJobResult> TryScheduleCronViaSdkAsync(string name, string schedule, DaprJobPayload payload, DateTimeOffset? startingFrom, DateTimeOffset? ttl)
    {
        var result = await ScheduleJobAsync(name, DaprJobSchedule.FromExpression(schedule), payload, startingFrom, ttl);
        if (result.Success)
        {
            _logger.LogInformation("DaprJobs schedule applied via SDK: {Schedule}", schedule);
        }
        return result;
    }

    private async Task<ScheduleJobResult> ScheduleJobAsync(string name, DaprJobSchedule schedule, DaprJobPayload payload, DateTimeOffset? startingFrom, DateTimeOffset? ttl)
    {
        try
        {
            ReadOnlyMemory<byte> payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
            await _daprJobsClient.ScheduleJobAsync(name, schedule, payloadBytes, startingFrom, repeats: null, ttl);
            return ScheduleJobResult.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DaprJobs schedule failed via SDK.");
            return ScheduleJobResult.Fail(ex.Message);
        }
    }

}
