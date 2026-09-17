// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Extensions;

public static class SupperAppConfigurationExtensions
{
    public static string? GetSsoHost(this IConfiguration configuration)
    {
        var json = GetValue(configuration);
        if (json == null || json.Count == 0)
            return default;
        return json.FirstOrDefault(x => x!["id"]?.ToString() == MasaStackProject.Auth.Name)?[MasaStackApp.SSO.Name]?["domain"]?.ToString();
    }

    public static string? GetProjectAppId(this IConfiguration configuration, MasaStackProject project, MasaStackApp app)
    {
        var json = GetValue(configuration);
        if (json == null || json.Count == 0)
            return default;
        return json.FirstOrDefault(i => i?["id"]?.ToString() == project.Name)?[app.Name]?["id"]?.ToString() ?? "";
    }

    private static JsonArray? GetValue(this IConfiguration configuration)
    {
        var value = configuration.GetValue<string>("MASA_STACK");
        if (string.IsNullOrEmpty(value))
            return default;
        return JsonSerializer.Deserialize<JsonArray>(value);
    }
}