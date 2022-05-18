// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StartTaskDomainEventHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IEventBus _eventBus;
    private readonly WorkerManager _workerManager;
    private readonly SchedulerDbContext _dbContext;
    private readonly IMapper _mapper;

    public StartTaskDomainEventHandler(ISchedulerTaskRepository schedulerTaskRepository, IEventBus eventBus, WorkerManager workerManager, SchedulerDbContext dbContext, IMapper mapper)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _eventBus = eventBus;
        _workerManager = workerManager;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [EventHandler(1)]
    public async Task StartTaskAsync(StartTaskDomainEvent @event)
    {
        SchedulerTask? task;

        if(@event.Task != null)
        {
            task = @event.Task;
        }
        else
        {
            task = await _dbContext.Tasks.Include(t => t.Job).FirstOrDefaultAsync(t => t.Id == @event.Request.TaskId);
        }

        if (task is null)
        {
            throw new UserFriendlyException($"Scheduler Task not found, Id: {@event.Request.TaskId}");
        }

        if (task.TaskStatus == TaskRunStatuses.Running)
        {
            throw new UserFriendlyException($"This task is running now, cannot run again");
        }

        if (task.Job is null)
        {
            throw new UserFriendlyException("SchedulerJob cannot null");
        }

        WorkerModel workerModel;

        if (task.Job.RoutingStrategy == RoutingStrategyTypes.Specified)
        {
            workerModel = await _workerManager.GetWorker(task.Job.SpecifiedWorkerHost);
        }
        else
        {
            workerModel = await _workerManager.GetWorker(task.Job.RoutingStrategy);
        }

        await _workerManager.StartTask(task, workerModel);

        task.TaskStart(workerModel.Host + ":" + workerModel.Port, @event.Request.OperatorId);

        await _schedulerTaskRepository.UpdateAsync(task);
    }
}
