// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public class SchedulerBackendOptions
{
    public const string SectionName = "Scheduler";

    public string Backend { get; set; } = SchedulerBackendType.Quartz;

    public DaprJobsOptions DaprJobs { get; set; } = new();

    public bool CleanupOtherBackendOnStart { get; set; }
}

public class DaprJobsOptions
{
    public bool Overwrite { get; set; } = true;
}

public static class SchedulerBackendType
{
    public const string Quartz = "Quartz";
    public const string DaprJobs = "DaprJobs";
}
