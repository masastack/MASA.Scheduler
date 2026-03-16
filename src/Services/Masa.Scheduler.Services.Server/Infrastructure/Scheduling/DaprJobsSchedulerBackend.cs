// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.Net;

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public class DaprJobsSchedulerBackend : ISchedulerBackend
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<SchedulerBackendOptions> _options;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;
    private readonly ILogger<DaprJobsSchedulerBackend> _logger;

    public DaprJobsSchedulerBackend(IHttpClientFactory httpClientFactory, IOptions<SchedulerBackendOptions> options, IMultiEnvironmentContext multiEnvironmentContext, ILogger<DaprJobsSchedulerBackend> logger)
    {
        _httpClientFactory = httpClientFactory;
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
            Environment = environment
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

        var dueTime = DateTimeOffset.UtcNow.Add(delay).ToString("O");
        var result = await ScheduleJobAsync(name, payload, schedule: null, dueTime: dueTime);
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
            var client = _httpClientFactory.CreateClient();
            var endpoint = BuildDaprHttpEndpoint($"/v1.0-alpha1/jobs/{Uri.EscapeDataString(name)}");
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Delete Dapr job failed. Name: {Name}. Status: {Status}. Body: {Body}", name, response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete Dapr job failed. Name: {Name}", name);
            throw;
        }
    }

    private async Task ScheduleCronJobAsync(string name, string cron, DaprJobPayload payload)
    {
        var candidates = BuildCronCandidates(cron);
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
            var result = await TryScheduleCronViaHttpAsync(name, candidate, payload);
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

    private static List<string> BuildCronCandidates(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron))
        {
            return new List<string>();
        }

        var normalized = cron.Trim().Replace('?', '*');
        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var candidates = new List<string>();

        AddCronCandidates(candidates, parts);

        if (parts.Any(part => part.Contains("0/")))
        {
            var variant = parts.Select(part => part.Replace("0/", "*/", StringComparison.Ordinal)).ToArray();
            AddCronCandidates(candidates, variant);
        }

        return candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static void AddCronCandidates(List<string> candidates, string[] parts)
    {
        if (parts.Length == 6)
        {
            var normalized = NormalizeDayOfWeek(parts, 5);
            candidates.Add(string.Join(' ', normalized.Skip(1)));
            candidates.Add(string.Join(' ', normalized));
        }
        else if (parts.Length == 7)
        {
            var normalized = NormalizeDayOfWeek(parts, 5);
            candidates.Add(string.Join(' ', normalized.Skip(1).Take(5)));
            candidates.Add(string.Join(' ', normalized.Skip(1).Take(6)));
        }
        else if (parts.Length == 5)
        {
            var normalized = NormalizeDayOfWeek(parts, 4);
            candidates.Add(string.Join(' ', normalized));
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

        if (value.Contains('7'))
        {
            var tokens = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(token =>
                {
                    var trimmed = token.Trim();
                    return string.Equals(trimmed, "7", StringComparison.Ordinal) ? "0" : trimmed;
                });
            value = string.Join(',', tokens.Distinct(StringComparer.Ordinal));
        }

        var copy = parts.ToArray();
        copy[index] = value;
        return copy;
    }

    private async Task<ScheduleJobResult> TryScheduleCronViaHttpAsync(string name, string schedule, DaprJobPayload payload)
    {
        var result = await ScheduleJobAsync(name, payload, schedule: schedule, dueTime: null);
        if (result.Success)
        {
            _logger.LogInformation("DaprJobs schedule applied via HTTP: {Schedule}", schedule);
        }
        return result;
    }

    private async Task<ScheduleJobResult> ScheduleJobAsync(string name, DaprJobPayload payload, string? schedule, string? dueTime)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var endpoint = BuildDaprHttpEndpoint($"/v1.0-alpha1/jobs/{Uri.EscapeDataString(name)}");
            var request = BuildScheduleRequest(payload, schedule, dueTime);
            var response = await client.PostAsJsonAsync(endpoint, request);
            if (response.IsSuccessStatusCode)
            {
                return ScheduleJobResult.SuccessResult();
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("DaprJobs schedule failed. Status: {Status}. Body: {Body}", response.StatusCode, error);
            return ScheduleJobResult.Fail(error);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DaprJobs schedule failed with exception.");
            return ScheduleJobResult.Fail(ex.Message);
        }
    }

    private object BuildScheduleRequest(DaprJobPayload payload, string? schedule, string? dueTime)
    {
        if (!string.IsNullOrWhiteSpace(dueTime))
        {
            return new
            {
                dueTime,
                data = payload,
                overwrite = _options.Value.DaprJobs.Overwrite
            };
        }

        return new
        {
            schedule,
            data = payload,
            overwrite = _options.Value.DaprJobs.Overwrite
        };
    }

    private static string BuildDaprHttpEndpoint(string path)
    {
        var endpoint = Environment.GetEnvironmentVariable("DAPR_HTTP_ENDPOINT");
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            var port = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";
            endpoint = $"http://127.0.0.1:{port}";
        }

        return $"{endpoint.TrimEnd('/')}{path}";
    }
}

public sealed record ScheduleJobResult(bool Success, string? Error)
{
    public static ScheduleJobResult SuccessResult() => new(true, null);

    public static ScheduleJobResult Fail(string? error) => new(false, error);
}

public sealed class DaprJobPayload
{
    public DaprJobNameType Type { get; set; }
    public Guid JobId { get; set; }
    public Guid? TaskId { get; set; }
    public string Environment { get; set; } = string.Empty;
    public DateTimeOffset? ExecuteTime { get; set; }
}
