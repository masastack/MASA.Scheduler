// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace System;

internal static class ObjectExtensions
{
    public static IDictionary<string, object> ToDictionary(this object source, params string[] excludedProperties)
    {
        return source.ToDictionary<object>(excludedProperties);
    }

    public static IDictionary<string, T> ToDictionary<T>(this object source, params string[] excludedProperties)
    {
        if (source == null) ThrowExceptionWhenSourceArgumentIsNull();

        var dictionary = new Dictionary<string, T>();
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source!))
        {
            if (excludedProperties.Contains(property.Name))
            {
                continue;
            }

            object value = property.GetValue(source!)!;
            if (IsOfType<T>(value))
            {
                dictionary.Add(property.Name, (T)value);
            }
        }

        return dictionary;
    }

    public static T Random<T>(this IEnumerable<T> source)
    {
        return source.ToArray()[System.Random.Shared.Next(source.Count())];
    }

    private static bool IsOfType<T>(object value)
    {
        return value is T;
    }

    private static void ThrowExceptionWhenSourceArgumentIsNull()
    {
        throw new UserFriendlyException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
    }

    public static string FormatTime(this double millionSeconds)
    {
        if (millionSeconds <= 1)
            return "<1ms";
        if (millionSeconds - 1000 < 0)
            return $"{millionSeconds:0.##}ms";
        if (millionSeconds - 60_000 < 0)
            return $"{millionSeconds / 1000:0.##} s";
        var minis = (long)millionSeconds / 60_000;
        var seconds = ((long)millionSeconds % 60_000) / 1000;
        return $"{minis} min{(seconds > 0 ? $"{seconds} s" : "")}";
    }

    public static int FormatTimeToNumber(this string s, bool isEnd = false)
    {
        if (string.IsNullOrEmpty(s))
            return 0;
        if (s == "<1ms")
            return 0;
        if (s.EndsWith("ms"))
            return Convert.ToInt32(s.Substring(0, s.Length - 2)) + (isEnd ? 1 : 0);
        if (s.EndsWith(" s"))
            return (int)(Convert.ToDouble(s.Substring(0, s.Length - 2)) * 1000) + (isEnd ? 100 : 0);
        if (s.EndsWith(" min"))
            return (int)(Convert.ToDouble(s.Substring(0, s.Length - 4)) * 1000);
        return default;
    }

    public static string FormatHistory(this DateTime time)
    {
        var now = DateTime.UtcNow;
        var timeSpan = now - time;
        var days = (int)Math.Floor(timeSpan.TotalDays);
        var housrs = timeSpan.Hours;
        var minutes = timeSpan.Minutes;

        int num = 0;
        string unit = "";
        if (days > 0)
        {
            if (days - 7 <= 0)
            {
                num = days;
                unit = "day";
            }
            else if (days - 25 <= 0)
            {
                num = (days / 7) + (days % 7 > 0 ? 1 : 0);
                unit = "week";
            }
            else if (days - 360 < 0)
            {
                num = (days / 30) + (days % 30 > 0 ? 1 : 0);
                unit = "month";
            }
            else
            {
                num = (days / 365) + (days % 365 > 0 ? 1 : 0);
                unit = "year";
            }
        }
        else
        {
            if (housrs > 0)
            {
                num = housrs;
                unit = "hour";
            }
            else if (minutes > 0)
            {
                num = minutes;
                unit = "minute";
            }
            else
            {
                num = time.Second;
                unit = "second";
            }
        }

        return $"{num} {unit}{(num == 1 ? "" : "s")} ago";
    }

    public static string ToUrlSafe(this string s)
    {
        return s;
    }
}
