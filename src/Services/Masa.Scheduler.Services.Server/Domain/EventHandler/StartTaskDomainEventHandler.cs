// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StartTaskDomainEventHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly IEventBus _eventBus;
    private readonly SchedulerServerManager _serverManager;
    private readonly SchedulerDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly QuartzUtils _quartzUtils;
    private readonly IDistributedCacheClient _distributedCacheClient;
    private readonly ILogger<StartTaskDomainEventHandler> _logger;

    public StartTaskDomainEventHandler(ISchedulerTaskRepository schedulerTaskRepository, IEventBus eventBus, SchedulerServerManager serverManager, SchedulerDbContext dbContext, IMapper mapper, QuartzUtils quartzUtils, IDistributedCacheClient distributedCacheClient, ILogger<StartTaskDomainEventHandler> logger, ISchedulerJobRepository schedulerJobRepository)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _eventBus = eventBus;
        _serverManager = serverManager;
        _dbContext = dbContext;
        _mapper = mapper;
        _quartzUtils = quartzUtils;
        _distributedCacheClient = distributedCacheClient;
        _logger = logger;
        _schedulerJobRepository = schedulerJobRepository;
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
            _logger.LogError($"SchedulerJob was disabled or deleted, taskId: {task.Id}, jobId: {task.JobId}");
            throw new UserFriendlyException($"SchedulerJob was disabled or deleted");
        }

        // When task is running, restart will stop task first
        if (task.TaskStatus == TaskRunStatus.Running)
        {
            await _serverManager.StopTask(task.Id, task.WorkerHost);
        }

        // When task run by manual, remove FailedStrategy delay task
        if (task.TaskStatus == TaskRunStatus.WaitToRetry && @event.Request.IsManual)
        {
            await _quartzUtils.RemoveDelayTask(task.Id, task.Job.Id);
            await _distributedCacheClient.RemoveAsync<int>($"{CacheKeys.TASK_RETRY_COUNT}_{task.Id}");
        }

        var filterStatus = new List<TaskRunStatus>() { TaskRunStatus.Running, TaskRunStatus.WaitToRetry, TaskRunStatus.Idle };

        var otherRunningTaskList = await _schedulerTaskRepository.GetListAsync(t => filterStatus.Contains(t.TaskStatus) && t.JobId == task.JobId && t.Id != task.Id);

        var allowEnqueue = true;

        task.UpdateTaskSchedulerTime(@event.Request.ExcuteTime);

        if (otherRunningTaskList.Any())
        {
            switch (task.Job.ScheduleBlockStrategy)
            {
                case ScheduleBlockStrategyTypes.Serial:
                    task.Wait();
                    allowEnqueue = false;
                    break;
                case ScheduleBlockStrategyTypes.Discard:
                    task.Discard();
                    allowEnqueue = false;
                    break;
                case ScheduleBlockStrategyTypes.Cover:
                    foreach (var otherRunningTask in otherRunningTaskList)
                    {
                        if (otherRunningTask.TaskStatus == TaskRunStatus.Running)
                        {
                            await _serverManager.StopTask(otherRunningTask.Id, otherRunningTask.WorkerHost);
                        }
                        else
                        {
                            await _quartzUtils.RemoveDelayTask(otherRunningTask.Id, task.Job.Id);
                        }

                        otherRunningTask.TaskEnd(TaskRunStatus.Failure, "Stop by SchedulerBlockStrategy");
                        await _schedulerTaskRepository.UpdateAsync(otherRunningTask);
                    }
                    task.TaskSchedule(@event.Request.OperatorId);
                    break;
                default:
                    task.TaskSchedule(@event.Request.OperatorId);
                    break;
            }
        }
        else
        {
            task.TaskSchedule(@event.Request.OperatorId);
        }

        if(task.TaskStatus != TaskRunStatus.WaitToRun)
        {
            task.Job.UpdateLastScheduleTime(@event.Request.ExcuteTime);
            task.Job.UpdateLastRunDetail(task.TaskStatus);
            await _schedulerJobRepository.UpdateAsync(task.Job);
        }

        await _schedulerTaskRepository.UpdateAsync(task);
        await _schedulerTaskRepository.UnitOfWork.SaveChangesAsync();
        await _schedulerTaskRepository.UnitOfWork.CommitAsync();

        if (allowEnqueue)
        {
            await _serverManager.TaskEnqueue(task);
        }
    }
}
