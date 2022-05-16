// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobAppConfig
{
    public Guid JobId { get; private set; }

    public int JobAppId { get; private set; }

    public string JobEntryAssembly { get; private set; } = string.Empty;

    public string JobEntryMethod { get; private set; } = string.Empty;

    public string JobParams { get; private set; } = string.Empty;

    public string Version { get; private set; } = string.Empty;

    public SchedulerJobAppConfig(Guid jobId, int jobAppId, string jobEntryAssembly, string jobEntryMethod, string jobParams, string version)
    {
        JobId = jobId;
        JobAppId = jobAppId;
        JobEntryAssembly = jobEntryAssembly;
        JobEntryMethod = jobEntryMethod;
        JobParams = jobParams;
        Version = version;
    }
}
