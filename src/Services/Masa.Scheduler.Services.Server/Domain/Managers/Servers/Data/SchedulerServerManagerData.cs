// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers.Data;

public class SchedulerServerManagerData : BaseSchedulerManagerData<WorkerModel>
{
    public Queue<TaskAssignModel> TaskQueue { get; set; } = new();
}
