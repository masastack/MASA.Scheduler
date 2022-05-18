// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StopTaskDomainEventHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IEventBus _eventBus;
    private readonly WorkerManager _workerManager;

    public StopTaskDomainEventHandler(ISchedulerTaskRepository schedulerTaskRepository, IEventBus eventBus, WorkerManager workerManager)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _eventBus = eventBus;
        _workerManager = workerManager;
    }

    [EventHandler(1)]
    public async Task StopTaskAsync(StopTaskDomainEvent @event)
    {
        var task = await _schedulerTaskRepository.FindAsync(t => t.Id == @event.Request.TaskId);

        if (task == null)
        {
            throw new UserFriendlyException($"Scheduler Task not found, Id: {@event.Request.TaskId}");
        }

        if(task.TaskStatus != TaskRunStatuses.Running)
        {
            throw new UserFriendlyException("Only running task can be stop");
        }

        await _workerManager.StopTask(task.Id, task.WorkerHost);

        if (!@event.IsRestart)
        {
            task.TaskEnd(TaskRunStatuses.Failure, $"User manual stop task, OperatorId: {@event.Request.OperatorId}");
            await _schedulerTaskRepository.UpdateAsync(task);
        }
    }
}
