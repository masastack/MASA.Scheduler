// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Extensions;

public static class MasaStackConfigExtensions
{
    public static string GetWorkerId(this IMasaStackConfig masaStackConfig, string project)
    {
        string project2 = project;
        return masaStackConfig.GetMasaStack().FirstOrDefault((JsonNode? i) => i?["id"]?.ToString() == project2)?["worker"]?["id"]?.ToString() ?? "";
    }
}