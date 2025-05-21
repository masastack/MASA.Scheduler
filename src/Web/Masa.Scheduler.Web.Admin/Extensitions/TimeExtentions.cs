// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace System;

internal static class TimeExtentions
{
    private const int MIN_STEP = 30;

    public static long ToUnixTimestamp(this DateTime time)
    {
        return new DateTimeOffset(time).ToUnixTimeSeconds();
    }

    public static DateTime ToDateTime(this long timestamp, TimeZoneInfo? timeZone = default)
    {
        DateTimeOffset offset;
        if (timestamp - 0x7ffffffff > 0)
            offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        else
            offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        if (timeZone != null && timeZone.BaseUtcOffset.TotalSeconds > 0)
        {
            return new DateTimeOffset(offset: timeZone.BaseUtcOffset, ticks: offset.Ticks + timeZone.BaseUtcOffset.Ticks).DateTime;
        }
        return offset.DateTime;
    }

    public static string Format(this DateTime time, string fmt = "yyyy-MM-dd HH:mm:ss")
    {
        if (time == DateTime.MinValue || time == DateTime.MaxValue)
            return "";
        return time.ToString(fmt);
    }

    public static string UtcFormatLocal(this DateTime time, TimeZoneInfo timeZoneInfo, string fmt = "yyyy-MM-dd HH:mm:ss")
    {
        if (time == DateTime.MinValue || time == DateTime.MaxValue)
            return "";
        return (time.ToUniversalTime() + timeZoneInfo.BaseUtcOffset).ToString(fmt);
    }


    /// <summary>
    /// prometheus step
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static string Interval(this DateTime start, DateTime end, string defaultInterval = "1m")
    {
        if (start == DateTime.MaxValue || start == DateTime.MinValue || start == end)
            return string.Empty;
        var total = (long)Math.Floor((end - start).TotalSeconds);
        var step = GetInvervalSecond(defaultInterval);
        var maxStep = total / 250;
        if (maxStep - MIN_STEP < 0)
            maxStep = MIN_STEP;
        if (maxStep - step <= 0)
            maxStep = step;
        return $"{maxStep}s";
    }

    private static long GetInvervalSecond(string value)
    {
        var num = Convert.ToInt64(value[..(value.Length - 1)]);
        return value[^1] switch
        {
            's' => num,
            'm' => num * 60,
            'h' => num * 3600,
            'w' => num * 3600 * 7,
            'y' => num * 3600 * 365,
            _ => 0,
        };
    }

    /// <summary>
    /// echart line bar time format
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static string Format(this DateTime start, DateTime end)
    {
        if (start == DateTime.MaxValue || start == DateTime.MinValue || start == end)
            return string.Empty;
        var timeSpan = end - start;

        if (timeSpan.TotalMinutes - 60 < 0)
            return "mm:ss";

        if (timeSpan.TotalHours - 24 <= 0)
            return "HH:mm:ss";

        return " dd HH:mm";
    }

    public static DateTimeOffset ToDateTimeOffset(this DateTime? time, TimeZoneInfo? timeZoneInfo)
    {
        if (time == null)
            return DateTimeOffset.MinValue;
        if (timeZoneInfo == null)
            timeZoneInfo = TimeZoneInfo.Utc;
        return new DateTimeOffset(ticks: time.Value.Ticks + timeZoneInfo.BaseUtcOffset.Ticks, offset: timeZoneInfo.BaseUtcOffset);
    }

    public static DateTimeOffset ToDateTimeOffset(this DateTime time, TimeZoneInfo? timeZoneInfo)
    {
        timeZoneInfo ??= TimeZoneInfo.Utc;
        return new DateTimeOffset(ticks: time.Ticks + timeZoneInfo.BaseUtcOffset.Ticks, offset: timeZoneInfo.BaseUtcOffset);
    }
}
