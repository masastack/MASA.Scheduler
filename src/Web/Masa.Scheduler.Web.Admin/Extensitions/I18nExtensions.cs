// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Blazor;

internal static class TscI18nExtensions
{
    public static string Team(this I18n? i18n, string key)
    {
        return i18n?.T("Team", key)!;
    }

    public static string Log(this I18n? i18n, string key)
    {
        return i18n?.T("Log", key)!;
    }

    public static string Trace(this I18n? i18n, string key)
    {
        return i18n?.T("Trace", key)!;
    }

    public static string Apm(this I18n? i18n, string key)
    {
        return i18n?.T("Apm", key)!;
    }

    public static string Dashboard(this I18n? i18n, string key)
    {
        return i18n?.T("Dashboard", key)!;
    }

    public static string TeamDashboard(this I18n? i18n, string key)
    {
        return i18n?.T("TeamDashboard", key)!;
    }

    public static string DateTimeRange(this I18n? i18n, string key)
    {
        return i18n?.T("DateTimeRange", key)!;
    }
}
