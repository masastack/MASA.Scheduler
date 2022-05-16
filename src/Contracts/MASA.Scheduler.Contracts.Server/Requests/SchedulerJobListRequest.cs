// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests;

public class SchedulerJobListRequest: BaseRequest
{
    public bool IsCreatedByManual { get; set; }

    public TaskRunStatuses FilterStatus { get; set; }

    public string JobName { get; set; } = string.Empty;

    public JobQueryTimeTypes QueryTimeType { get; set; }

    public DateTimeOffset? QueryStartTime { get; set; }

    public DateTimeOffset? QueryEndTime { get; set; }

    public JobTypes JobType { get; set; }

    public string Origin { get; set; } = string.Empty;

    public int Page { get; set; }

    public int PageSize { get; set; }
}

