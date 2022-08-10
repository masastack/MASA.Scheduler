// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StopTaskDomainEventHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly IEventBus _eventBus;
    private readonly SchedulerServerManager _serverManager;

    public StopTaskDomainEventHandler(
        ISchedulerTaskRepository schedulerTaskRepository,
        IEventBus eventBus,
        SchedulerServerManager serverManager,
        ISchedulerJobRepository schedulerJobRepository)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _eventBus = eventBus;
        _serverManager = serverManager;
        _schedulerJobRepository = schedulerJobRepository;
    }

    [EventHandler(1)]
    public async Task StopTaskAsync(StopTaskDomainEvent @event)
    {
        var task = await _schedulerTaskRepository.FindAsync(t => t.Id == @event.Request.TaskId);

        if (task == null)
        {
            throw new UserFriendlyException($"Scheduler Task not found, Id: {@event.Request.TaskId}");
        }

        if(task.TaskStatus != TaskRunStatus.Running && task.TaskStatus != TaskRunStatus.WaitToRun && task.TaskStatus != TaskRunStatus.WaitToRetry && task.TaskStatus != TaskRunStatus.Timeout)
        {
            throw new UserFriendlyException("Only Process status can be stop");
        }

        if(task.TaskStatus == TaskRunStatus.Running || task.TaskStatus == TaskRunStatus.Timeout)
        {
            await _serverManager.StopTask(task.Id, task.WorkerHost);
        }

        if (!@event.IsRestart)
        {
            var notifyEvent = new NotifyTaskRunResultDomainEvent(new NotifySchedulerTaskRunResultRequest()
            {
                Status = TaskRunStatus.Failure,
                TaskId = task.Id,
                Message = $"User manual stop task, OperatorId: {@event.Request.OperatorId}"
            });

            await _eventBus.PublishAsync(notifyEvent);
        }
    }
}
