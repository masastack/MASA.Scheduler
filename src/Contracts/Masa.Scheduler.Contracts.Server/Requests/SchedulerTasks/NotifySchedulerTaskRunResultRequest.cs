// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;

public class NotifySchedulerTaskRunResultRequest : BaseRequest
{
    public Guid TaskId { get; set; }

    public TaskRunStatus Status { get; set; }

    public string? Message { get; set; }

    public bool StopManaul { get; set; }
}
