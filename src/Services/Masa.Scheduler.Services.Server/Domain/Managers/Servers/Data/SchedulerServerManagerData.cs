// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers.Data;

public class SchedulerServerManagerData : BaseSchedulerManagerData<WorkerModel>
{
    public Queue<SchedulerTaskDto> TaskQueue { get; set; } = new();

    public List<Guid> StopByManual { get; set; } = new();
}
