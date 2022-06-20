// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StartTaskDomainEventHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IEventBus _eventBus;
    private readonly SchedulerServerManager _serverManager;
    private readonly SchedulerDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly QuartzUtils _quartzUtils;
    private readonly IDistributedCacheClient _distributedCacheClient;

    public StartTaskDomainEventHandler(ISchedulerTaskRepository schedulerTaskRepository, IEventBus eventBus, SchedulerServerManager serverManager, SchedulerDbContext dbContext, IMapper mapper, QuartzUtils quartzUtils, IDistributedCacheClient distributedCacheClient)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _eventBus = eventBus;
        _serverManager = serverManager;
        _dbContext = dbContext;
        _mapper = mapper;
        _quartzUtils = quartzUtils;
        _distributedCacheClient = distributedCacheClient;
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

        if (task.Job is null)
        {
            throw new UserFriendlyException("SchedulerJob cannot null");
        }

        if(!task.Job.Enabled || task.Job.IsDeleted)
        {
            throw new UserFriendlyException($"SchedulerJob was disabled or deleted");
        }

        if (task.TaskStatus == TaskRunStatus.Running)
        {
            await _serverManager.StopTask(task.Id, task.WorkerHost);
        }

        if(task.TaskStatus == TaskRunStatus.WaitForRetry)
        {
            await _quartzUtils.RemoveDelayTask(task.Id, task.Job.Id);
            await _distributedCacheClient.RemoveAsync<int>($"{CacheKeys.TASK_RETRY_COUNT}_{task.Id}");
        }

        task.TaskSchedule(@event.Request.OperatorId);

        await _schedulerTaskRepository.UpdateAsync(task);
        await _schedulerTaskRepository.UnitOfWork.SaveChangesAsync();

        await _serverManager.TaskEnqueue(task);
    }
}
