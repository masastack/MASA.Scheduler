// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;

public class SchedulerJobListRequest: PaginationRequest
{
    public bool IsCreatedByManual { get; set; }

    public TaskRunStatus FilterStatus { get; set; }

    public string JobName { get; set; } = string.Empty;

    public JobQueryTimeTypes QueryTimeType { get; set; }

    public DateTimeOffset? QueryStartTime { get; set; }

    public DateTimeOffset? QueryEndTime { get; set; }

    public string BelongProjectIdentity { get; set; } = string.Empty;

    public JobTypes JobType { get; set; }

    public string Origin { get; set; } = string.Empty;
}

