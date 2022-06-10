// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StopTaskDomainEventHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly IEventBus _eventBus;
    private readonly SchedulerServerManager _serverManager;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public StopTaskDomainEventHandler(ISchedulerTaskRepository schedulerTaskRepository, IEventBus eventBus, SchedulerServerManager serverManager, IHubContext<NotificationsHub> hubContext, ISchedulerJobRepository schedulerJobRepository)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _eventBus = eventBus;
        _serverManager = serverManager;
        _hubContext = hubContext;
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

        if(task.TaskStatus != TaskRunStatus.Running)
        {
            throw new UserFriendlyException("Only running task can be stop");
        }

        await _serverManager.StopTask(task.Id, task.WorkerHost);

        if (!@event.IsRestart)
        {
            task.TaskEnd(TaskRunStatus.Failure, $"User manual stop task, OperatorId: {@event.Request.OperatorId}");

            await _schedulerTaskRepository.UpdateAsync(task);

            var job = await _schedulerJobRepository.FindAsync(j => j.Id == task.JobId);

            if(job != null)
            {
                job.UpdateLastRunDetail(TaskRunStatus.Failure);
                await _schedulerJobRepository.UpdateAsync(job);
            }
       
            await _schedulerTaskRepository.UnitOfWork.SaveChangesAsync();
            await _schedulerTaskRepository.UnitOfWork.CommitAsync();

            var groupClient = _hubContext.Clients.Group(ConstStrings.GLOBAL_GROUP);
            await groupClient.SendAsync(SignalRMethodConsts.GET_NOTIFICATION);
        }
    }
}
