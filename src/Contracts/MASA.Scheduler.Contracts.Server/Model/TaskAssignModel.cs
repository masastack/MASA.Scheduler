// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Model;

public class TaskAssignModel
{
    public WorkerModel Worker { get; set; } = default!;

    public SchedulerTaskDto Task { get; set; } = default!;
}
