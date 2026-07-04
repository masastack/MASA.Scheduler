// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public static class DaprJobsCronTimeZoneConverter
{
    public static List<string> ConvertCronCandidatesToSchedulerTimeZone(List<string> candidates, string cronTimeZone)
    {
        // Dapr Jobs schedule runs in scheduler host timezone and does not support
        // CRON_TZ/TZ prefixes consistently. Convert schedule into UTC explicitly.
        var sourceTimeZone = ResolveCronTimeZone(cronTimeZone);
        var targetTimeZone = TimeZoneInfo.Utc;

        var nowUtc = DateTimeOffset.UtcNow.UtcDateTime;
        if (HasOffsetShiftWithinHorizon(sourceTimeZone, nowUtc, 400) || HasOffsetShiftWithinHorizon(targetTimeZone, nowUtc, 400))
        {
            throw new UserFriendlyException($"DaprJobs timezone conversion does not support time zones whose UTC offset changes over time. Source: {sourceTimeZone.Id}, Target: {targetTimeZone.Id}");
        }

        var sourceOffset = sourceTimeZone.GetUtcOffset(nowUtc);
        var targetOffset = targetTimeZone.GetUtcOffset(nowUtc);
        var offsetDelta = targetOffset - sourceOffset;

        if (offsetDelta.Ticks % TimeSpan.TicksPerHour != 0)
        {
            throw new UserFriendlyException($"DaprJobs timezone conversion only supports whole-hour offsets. Source: {sourceTimeZone.Id}, Target: {targetTimeZone.Id}");
        }

        var hourDelta = (int)(offsetDelta.Ticks / TimeSpan.TicksPerHour);
        if (hourDelta == 0)
        {
            return candidates;
        }

        return candidates
            .SelectMany(candidate => ConvertCronExpressionToSchedulerTimeZone(candidate, hourDelta))
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static TimeZoneInfo ResolveCronTimeZone(string? cronTimeZone)
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

    public static string ExtractCronBodyAndResolveTimeZone(string cron, string? defaultCronTimeZone, out string? resolvedCronTimeZone)
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

    public static CronActivationWindow BuildCronActivationWindow(string cron, string? defaultCronTimeZone)
    {
        var cronExpression = ExtractCronBodyAndResolveTimeZone(cron, defaultCronTimeZone, out var resolvedCronTimeZone);
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

    private static List<string> ConvertCronExpressionToSchedulerTimeZone(string cronExpression, int hourDelta)
    {
        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 6)
        {
            throw new UserFriendlyException($"DaprJobs does not support cron expression after timezone conversion: {cronExpression}");
        }

        var hourGroups = ShiftHourTokenWithDayGroups(parts[2], hourDelta);
        var converted = new List<string>();
        foreach (var hourGroup in hourGroups)
        {
            var copy = parts.ToArray();
            copy[2] = hourGroup.HourToken;
            ApplyDayShiftForConvertedCron(copy, hourGroup.DayShift, cronExpression);
            converted.Add(string.Join(' ', copy));
        }

        return converted;
    }

    private static void ApplyDayShiftForConvertedCron(string[] cronParts, int dayShift, string originalCronExpression)
    {
        if (dayShift == 0)
        {
            return;
        }

        // For day-boundary shifts, day-of-month/month constrained cron cannot be
        // represented safely after timezone conversion with a single cron expression.
        if (!IsWildcardToken(cronParts[3]) || !IsWildcardToken(cronParts[4]))
        {
            throw new UserFriendlyException($"DaprJobs timezone conversion does not support day/month constrained cron crossing day boundary: {originalCronExpression}");
        }

        if (!IsWildcardToken(cronParts[5]))
        {
            cronParts[5] = ShiftDayOfWeekToken(cronParts[5], dayShift);
        }
    }

    private static bool IsWildcardToken(string token)
    {
        return string.Equals(token, "*", StringComparison.Ordinal);
    }

    private static List<(int DayShift, string HourToken)> ShiftHourTokenWithDayGroups(string hourToken, int hourDelta)
    {
        var parsedHours = ParseCronFieldToken(hourToken, 0, 23, aliasResolver: null);
        if (parsedHours.IsAll || hourDelta == 0)
        {
            return new List<(int DayShift, string HourToken)> { (0, "*") };
        }

        var groupedHours = new Dictionary<int, SortedSet<int>>();
        foreach (var hour in parsedHours.Values)
        {
            var shiftedHour = hour + hourDelta;
            var dayShift = FloorDiv(shiftedHour, 24);
            var normalizedHour = shiftedHour - dayShift * 24;

            if (!groupedHours.TryGetValue(dayShift, out var hourSet))
            {
                hourSet = new SortedSet<int>();
                groupedHours[dayShift] = hourSet;
            }

            hourSet.Add(normalizedHour);
        }

        return groupedHours
            .OrderBy(item => item.Key)
            .Select(item => (item.Key, FormatCronFieldToken(item.Value, 0, 23)))
            .ToList();
    }

    private static int FloorDiv(int value, int divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor));
        }

        var quotient = value / divisor;
        var remainder = value % divisor;
        if (remainder < 0)
        {
            quotient--;
        }

        return quotient;
    }

    private static string ShiftDayOfWeekToken(string dayOfWeekToken, int dayShift)
    {
        var parsedDayOfWeek = ParseCronFieldToken(dayOfWeekToken, 0, 6, ResolveDayOfWeekAlias);
        if (parsedDayOfWeek.IsAll)
        {
            return "*";
        }

        var shiftedValues = parsedDayOfWeek.Values
            .Select(value => NormalizeToRange(value + dayShift, 0, 6));

        return FormatCronFieldToken(shiftedValues, 0, 6);
    }

    private static int NormalizeToRange(int value, int min, int max)
    {
        var size = max - min + 1;
        var normalized = (value - min) % size;
        if (normalized < 0)
        {
            normalized += size;
        }

        return normalized + min;
    }

    private static int? ResolveDayOfWeekAlias(string token)
    {
        return token.ToUpperInvariant() switch
        {
            "SUN" => 0,
            "MON" => 1,
            "TUE" => 2,
            "WED" => 3,
            "THU" => 4,
            "FRI" => 5,
            "SAT" => 6,
            _ => null
        };
    }

    private static (bool IsAll, SortedSet<int> Values) ParseCronFieldToken(string token, int min, int max, Func<string, int?>? aliasResolver)
    {
        if (string.Equals(token, "*", StringComparison.Ordinal))
        {
            return (true, new SortedSet<int>(Enumerable.Range(min, max - min + 1)));
        }

        var values = new SortedSet<int>();
        foreach (var segment in token.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            ParseCronFieldSegment(values, segment, min, max, aliasResolver);
        }

        if (values.Count == 0)
        {
            throw new UserFriendlyException($"DaprJobs does not support cron field token: {token}");
        }

        var isAll = values.Count == max - min + 1;
        return (isAll, values);
    }

    private static void ParseCronFieldSegment(SortedSet<int> values, string segment, int min, int max, Func<string, int?>? aliasResolver)
    {
        var stepParts = segment.Split('/', StringSplitOptions.TrimEntries);
        if (stepParts.Length > 2)
        {
            throw new UserFriendlyException($"DaprJobs does not support cron segment: {segment}");
        }

        var step = 1;
        if (stepParts.Length == 2 && (!int.TryParse(stepParts[1], out step) || step <= 0))
        {
            throw new UserFriendlyException($"DaprJobs does not support cron step segment: {segment}");
        }

        var baseToken = stepParts[0];
        var baseValues = ExpandCronFieldBaseToken(baseToken, min, max, aliasResolver);
        if (baseValues.Count == 0)
        {
            throw new UserFriendlyException($"DaprJobs does not support cron segment: {segment}");
        }

        if (step == 1)
        {
            foreach (var value in baseValues)
            {
                values.Add(value);
            }

            return;
        }

        if (!string.Equals(baseToken, "*", StringComparison.Ordinal) && !baseToken.Contains('-', StringComparison.Ordinal))
        {
            var start = ParseCronFieldValue(baseToken, min, max, aliasResolver);
            for (var value = start; value <= max; value += step)
            {
                values.Add(value);
            }

            return;
        }

        for (var index = 0; index < baseValues.Count; index += step)
        {
            values.Add(baseValues[index]);
        }
    }

    private static List<int> ExpandCronFieldBaseToken(string baseToken, int min, int max, Func<string, int?>? aliasResolver)
    {
        if (string.Equals(baseToken, "*", StringComparison.Ordinal))
        {
            return Enumerable.Range(min, max - min + 1).ToList();
        }

        var rangeParts = baseToken.Split('-', StringSplitOptions.TrimEntries);
        if (rangeParts.Length == 1)
        {
            var singleValue = ParseCronFieldValue(baseToken, min, max, aliasResolver);
            return new List<int> { singleValue };
        }

        if (rangeParts.Length != 2)
        {
            throw new UserFriendlyException($"DaprJobs does not support cron range token: {baseToken}");
        }

        var start = ParseCronFieldValue(rangeParts[0], min, max, aliasResolver);
        var end = ParseCronFieldValue(rangeParts[1], min, max, aliasResolver);

        if (start <= end)
        {
            return Enumerable.Range(start, end - start + 1).ToList();
        }

        // Wrapped range support, e.g. 20-2.
        return Enumerable.Range(start, max - start + 1)
            .Concat(Enumerable.Range(min, end - min + 1))
            .ToList();
    }

    private static int ParseCronFieldValue(string token, int min, int max, Func<string, int?>? aliasResolver)
    {
        if (int.TryParse(token, out var value))
        {
            if (value == 7 && min == 0 && max == 6)
            {
                return 0;
            }

            if (value >= min && value <= max)
            {
                return value;
            }
        }

        if (aliasResolver != null)
        {
            var aliasValue = aliasResolver(token);
            if (aliasValue.HasValue && aliasValue.Value >= min && aliasValue.Value <= max)
            {
                return aliasValue.Value;
            }
        }

        throw new UserFriendlyException($"DaprJobs does not support cron token value: {token}");
    }

    private static string FormatCronFieldToken(IEnumerable<int> values, int min, int max)
    {
        var orderedValues = values
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        if (orderedValues.Count == 0)
        {
            throw new UserFriendlyException("DaprJobs converted cron field is empty");
        }

        if (orderedValues.Count == max - min + 1)
        {
            return "*";
        }

        var segments = new List<string>();
        var rangeStart = orderedValues[0];
        var previous = orderedValues[0];

        for (var index = 1; index < orderedValues.Count; index++)
        {
            var current = orderedValues[index];
            if (current == previous + 1)
            {
                previous = current;
                continue;
            }

            segments.Add(rangeStart == previous ? rangeStart.ToString() : $"{rangeStart}-{previous}");
            rangeStart = current;
            previous = current;
        }

        segments.Add(rangeStart == previous ? rangeStart.ToString() : $"{rangeStart}-{previous}");
        return string.Join(',', segments);
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

    private static bool HasOffsetShiftWithinHorizon(TimeZoneInfo timeZone, DateTime utcStart, int days)
    {
        var baselineOffset = timeZone.GetUtcOffset(utcStart);
        for (var day = 1; day <= days; day++)
        {
            var offset = timeZone.GetUtcOffset(utcStart.AddDays(day));
            if (offset != baselineOffset)
            {
                return true;
            }
        }

        return false;
    }
}
