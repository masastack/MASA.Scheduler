// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobAppConfig
{
    public string JobEntryAssembly { get; private set; } = string.Empty;

    public string JobEntryMethod { get; private set; } = string.Empty;

    public string JobParams { get; private set; } = string.Empty;

    public string Version { get; private set; } = string.Empty;

    public SchedulerJobAppConfig(string jobEntryAssembly, string jobEntryMethod, string jobParams, string version)
    {
        JobEntryAssembly = jobEntryAssembly;
        JobEntryMethod = jobEntryMethod;
        JobParams = jobParams;
        Version = version;
    }
}
