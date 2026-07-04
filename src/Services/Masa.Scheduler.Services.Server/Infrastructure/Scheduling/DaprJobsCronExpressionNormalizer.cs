// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public static class DaprJobsCronExpressionNormalizer
{
    public static List<string> BuildCronCandidates(string cron, string? cronTimeZone)
    {
        if (string.IsNullOrWhiteSpace(cron))
        {
            return new List<string>();
        }

        var cronExpression = DaprJobsCronTimeZoneConverter.ExtractCronBodyAndResolveTimeZone(cron, cronTimeZone, out var resolvedCronTimeZone);
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

        return DaprJobsCronTimeZoneConverter.ConvertCronCandidatesToSchedulerTimeZone(normalizedCandidates, resolvedCronTimeZone);
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
}
