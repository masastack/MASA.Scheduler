// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public static class DaprJobsCronExpressionNormalizer
{
    private static readonly TimeSpan BeijingUtcOffset = TimeSpan.FromHours(8);

    public static List<string> BuildCronCandidates(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron))
        {
            return new List<string>();
        }

        ThrowIfCronTimeZonePrefix(cron);

        var normalized = cron.Trim().Replace('?', '*');
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

        return candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static CronActivationWindow BuildCronActivationWindow(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron))
        {
            return CronActivationWindow.Empty;
        }

        ThrowIfCronTimeZonePrefix(cron);

        var parts = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
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

        var dueTime = CreateBeijingDateTime(yearRange.Value.Min, 1, 1);
        var now = DateTimeOffset.Now;
        DateTimeOffset? startingFrom = dueTime > now ? dueTime : null;
        DateTimeOffset? ttl = null;
        if (yearRange.Value.Max < 9999)
        {
            ttl = CreateBeijingDateTime(yearRange.Value.Max + 1, 1, 1);
        }

        return new CronActivationWindow(startingFrom, ttl);
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

    private static void ThrowIfCronTimeZonePrefix(string cron)
    {
        var firstToken = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (firstToken != null
            && (firstToken.StartsWith("CRON_TZ=", StringComparison.OrdinalIgnoreCase)
                || firstToken.StartsWith("TZ=", StringComparison.OrdinalIgnoreCase)))
        {
            throw new UserFriendlyException("CronExpression should not include timezone prefix. DaprJobs backend applies Asia/Shanghai automatically");
        }
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

    private static DateTimeOffset CreateBeijingDateTime(int year, int month, int day)
    {
        var localDateTime = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
        return new DateTimeOffset(localDateTime, BeijingUtcOffset);
    }
}
