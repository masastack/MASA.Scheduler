// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;

public class SchedulerTaskListRequest : PaginationRequest
{
    public Guid JobId { get; set; }

    public TaskRunStatus FilterStatus { get; set; }

    public JobQueryTimeTypes QueryTimeType { get; set; }

    public DateTimeOffset? QueryStartTime { get; set; }

    public DateTimeOffset? QueryEndTime { get; set; }

    public string Origin { get; set; } = string.Empty;
}
