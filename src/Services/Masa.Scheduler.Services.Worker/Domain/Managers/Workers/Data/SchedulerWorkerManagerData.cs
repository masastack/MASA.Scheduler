// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.Data;

public class SchedulerWorkerManagerData : BaseSchedulerManagerData<ServerModel>
{
    public List<Guid> StopTask { get; set; } = new List<Guid>();
    public ConcurrentDictionary<string, ConcurrentQueue<TaskRunModel>> TaskQueue { get; set; } = new();

    public ConcurrentDictionary<Guid, CancellationTokenSource> TaskCancellationTokenSources = new();

    public ConcurrentDictionary<Guid, CancellationTokenSource> InternalCancellationTokenSources = new();
}
