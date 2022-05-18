// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerTasks;

public class RestartSchedulerTaskRequest : BaseRequest
{
    public Guid TaskId { get; set; }

    public Guid OperatorId { get; set; }
}

