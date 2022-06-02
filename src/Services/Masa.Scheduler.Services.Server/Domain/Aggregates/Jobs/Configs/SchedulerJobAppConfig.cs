// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobAppConfig: ValueObject
{
    [JsonInclude]
    public int JobAppId { get; set; }

    [JsonInclude]
    public string JobEntryAssembly { get; private set; } = string.Empty;

    [JsonInclude]
    public string JobEntryMethod { get; private set; } = string.Empty;

    [JsonInclude]
    public string JobParams { get; private set; } = string.Empty;

    [JsonInclude]
    public string Version { get; private set; } = string.Empty;

    public void SetConfig(int jobAppId, string jobEntryAssembly, string jobEntryMethod, string jobParams, string version)
    {
        JobAppId = jobAppId;
        JobEntryAssembly = jobEntryAssembly;
        JobEntryMethod = jobEntryMethod;
        JobParams = jobParams;
        Version = version;
    }

    protected override IEnumerable<object> GetEqualityValues()
    {
        yield return JobAppId;
        yield return JobEntryAssembly;
        yield return JobEntryMethod;
        yield return JobParams;
        yield return Version;
    }
}
