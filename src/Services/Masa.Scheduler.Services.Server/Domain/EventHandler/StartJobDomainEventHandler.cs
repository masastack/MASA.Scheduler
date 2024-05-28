// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StartJobDomainEventHandler
{
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IEventBus _eventBus;
    private readonly SchedulerLogger _logger;
    private readonly IUnitOfWork _unitOfWork;

    public StartJobDomainEventHandler(
        IEventBus eventBus,
        ISchedulerJobRepository schedulerJobRepository,
        ISchedulerTaskRepository schedulerTaskRepository,
        SchedulerLogger logger,
        IUnitOfWork unitOfWork)
    {
        _eventBus = eventBus;
        _schedulerJobRepository = schedulerJobRepository;
        _schedulerTaskRepository = schedulerTaskRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
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

        var task = new SchedulerTask(job.Id, job.Origin, operatorId, @event.Request.ExcuteTime);

        await _schedulerTaskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Scheduler Task Create", WriterTypes.Server, task.Id, task.JobId);

        var startTaskRequest = new StartSchedulerTaskRequest()
        {
            TaskId = task.Id,
            OperatorId = operatorId,
            ExcuteTime = @event.Request.ExcuteTime
        };

        await _eventBus.PublishAsync(new StartTaskDomainEvent(startTaskRequest, task));
    }
}
