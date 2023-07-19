// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Store;

public class SchedulerJobsState
{
    public TaskRunStatus QueryStatus { get; set; }

    public string QueryJobName { get; set; } = string.Empty;

    public JobTypes QueryJobType { get; set; }

    public string QueryOrigin { get; set; } = string.Empty;

    public string ProjectIdentity { get; set; } = string.Empty;

    public JobCreateTypes JobCreateType { get; set; } = JobCreateTypes.Manual;
}
