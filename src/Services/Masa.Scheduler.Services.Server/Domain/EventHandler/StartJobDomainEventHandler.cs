// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StartJobDomainEventHandler
{
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IEventBus _eventBus;

    public StartJobDomainEventHandler(
        IEventBus eventBus,
        ISchedulerJobRepository schedulerJobRepository,
        ISchedulerTaskRepository schedulerTaskRepository)
    {
        _eventBus = eventBus;
        _schedulerJobRepository = schedulerJobRepository;
        _schedulerTaskRepository = schedulerTaskRepository;
    }

    [EventHandler(1)]
    public async Task StartJobAsync(StartJobDomainEvent @event)
    {
        var job = await _schedulerJobRepository.FindAsync(j => j.Id == @event.Request.JobId);

        if (job is null)
        {
            throw new UserFriendlyException($"SchedulerJob not found, JobId: {@event.Request.JobId}");
        }

        var operatorId = @event.Request.OperatorId == default ? job.Creator : @event.Request.OperatorId;

        var task = new SchedulerTask(job.Id, job.Origin, operatorId);

        await _schedulerTaskRepository.AddAsync(task);

        await _schedulerTaskRepository.UnitOfWork.SaveChangesAsync();

        var startTaskRequest = new StartSchedulerTaskRequest()
        {
            TaskId = task.Id,
            OperatorId = operatorId,
            ExcuteTime = @event.Request.ExcuteTime
        };

        await _eventBus.PublishAsync(new StartTaskDomainEvent(startTaskRequest, task));
    }
}
