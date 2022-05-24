// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Application.Tasks;

public class TaskCommandHandler
{
    private readonly SchedulerWorkerManager _schedulerWorkerManager;

    public TaskCommandHandler(SchedulerWorkerManager schedulerWorkerManager)
    {
        _schedulerWorkerManager = schedulerWorkerManager;
    }

    [EventHandler]
    public async Task StartTaskHandleAsync(StartTaskCommand command)
    {
        await _schedulerWorkerManager.StartTaskAsync(command.Request.TaskId, command.Request.Job);
    }

    [EventHandler]
    public async Task StopTaskHandleAsync(StopTaskCommand command)
    {
        await _schedulerWorkerManager.StopTaskAsync(command.Request.TaskId);
    }
}
