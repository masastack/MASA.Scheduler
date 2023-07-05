// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;

public class NotifySchedulerTaskRunResultBySdkRequest : BaseRequest
{
    public Guid TaskId { get; set; }

    public TaskRunResultStatus Status { get; set; }

    public string? Message { get; set; }
}
