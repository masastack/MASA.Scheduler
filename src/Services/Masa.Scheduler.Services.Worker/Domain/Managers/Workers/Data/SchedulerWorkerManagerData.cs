// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.Data;

public class SchedulerWorkerManagerData : BaseSchedulerManagerData<ServerModel>
{
    public ConcurrentQueue<TaskRunModel> TaskQueue { get; set; } = new ();

    public Dictionary<Guid, CancellationTokenSource> TaskCancellationTokenSources = new();

    public Dictionary<Guid, CancellationTokenSource> InternalCancellationTokenSources = new();
}
