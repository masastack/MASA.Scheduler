// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobAppConfig: ValueObject
{
    [JsonInclude]
    public string JobAppIdentity { get; set; } = string.Empty;

    [JsonInclude]
    public string JobEntryAssembly { get; private set; } = string.Empty;

    [JsonInclude]
    public string JobEntryClassName { get; private set; } = string.Empty;

    [JsonInclude]
    public string JobParams { get; private set; } = string.Empty;

    [JsonInclude]
    public string Version { get; private set; } = string.Empty;

    public void SetConfig(string jobAppIdentity, string jobEntryAssembly, string jobEntryClassName, string jobParams, string version)
    {
        JobAppIdentity = jobAppIdentity;
        JobEntryAssembly = jobEntryAssembly;
        JobEntryClassName = jobEntryClassName;
        JobParams = jobParams;
        Version = version;
    }

    protected override IEnumerable<object> GetEqualityValues()
    {
        yield return JobAppIdentity;
        yield return JobEntryAssembly;
        yield return JobEntryClassName;
        yield return JobParams;
        yield return Version;
    }
}
