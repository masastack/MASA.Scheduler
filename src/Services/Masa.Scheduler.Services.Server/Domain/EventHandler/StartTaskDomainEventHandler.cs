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
    private readonly SchedulerLogger _logger;
    private readonly SignalRUtils _signalRUtils;
    private readonly IUnitOfWork _unitOfWork;

    public StartTaskDomainEventHandler(
        ISchedulerTaskRepository schedulerTaskRepository,
        IEventBus eventBus,
        SchedulerServerManager serverManager,
        SchedulerDbContext dbContext,
        IMapper mapper,
        QuartzUtils quartzUtils,
        IDistributedCacheClient distributedCacheClient,
        SchedulerLogger logger,
        ISchedulerJobRepository schedulerJobRepository,
        SignalRUtils signalRUtils,
        IUnitOfWork unitOfWork)
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
        _signalRUtils = signalRUtils;
        _unitOfWork = unitOfWork;
    }

    [EventHandler(1)]
    public async Task StartTaskAsync(StartTaskDomainEvent @event)
    {
        SchedulerTask? task;

        if (@event.Task != null)
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

        if (!task.Job.Enabled || task.Job.IsDeleted)
        {
            _logger.LogInformation("Scheduler Job is disable or delete, cancel this task", WriterTypes.Server, task.Id, task.JobId);

            var notifyEvent = new NotifyTaskRunResultDomainEvent(new NotifySchedulerTaskRunResultRequest()
            {
                TaskId = task.Id,
                Status = TaskRunStatus.Failure,
                StopManaul = true
            });

            await _eventBus.PublishAsync(notifyEvent);
            return;
        }

        // When task is running, restart will stop task first
        if (task.TaskStatus == TaskRunStatus.Running)
        {
            _logger.LogInformation("Scheduler Task is running, stop this task", WriterTypes.Server, task.Id, task.JobId);
            await _serverManager.StopTask(task.Id, task.WorkerHost);
        }

        // When task run by manual, remove FailedStrategy delay task
        if (task.TaskStatus == TaskRunStatus.WaitToRetry && @event.Request.IsManual)
        {
            await _quartzUtils.RemoveDelayTask(task.Id, task.Job.Id);
            await _distributedCacheClient.RemoveAsync($"{CacheKeys.TASK_RETRY_COUNT}_{task.Id}");
        }

        var filterStatus = new List<TaskRunStatus>() { TaskRunStatus.Running, TaskRunStatus.WaitToRetry, TaskRunStatus.Idle, TaskRunStatus.WaitToRun };

        var otherRunningTaskList = await _schedulerTaskRepository.GetPaginatedListAsync(t => filterStatus.Contains(t.TaskStatus) && t.JobId == task.JobId && t.Id != task.Id, 0, 10);

        var allowEnqueue = true;

        if (task.SchedulerTime == DateTimeOffset.MinValue)
        {
            task.UpdateTaskSchedulerTime(@event.Request.ExcuteTime);
        }

        if (otherRunningTaskList.Any())
        {
            switch (task.Job.ScheduleBlockStrategy)
            {
                case ScheduleBlockStrategyTypes.Serial:
                    if (otherRunningTaskList.Any(p => p.TaskStatus == TaskRunStatus.Running || p.TaskStatus == TaskRunStatus.WaitToRetry))
                    {
                        task.Wait();
                        allowEnqueue = false;
                        _logger.LogWarning("Other task is running, trigger serial block strategy, waiting now", WriterTypes.Server, task.Id, task.JobId);
                    }
                    break;
                case ScheduleBlockStrategyTypes.Discard:
                    task.Discard();
                    allowEnqueue = false;
                    _logger.LogWarning("Trigger discard block strategy, task failed", WriterTypes.Server, task.Id, task.JobId);
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

                        await _signalRUtils.SendNoticationByGroup(ConstStrings.GLOBAL_GROUP, SignalRMethodConsts.GET_NOTIFICATION, _mapper.Map<SchedulerTaskDto>(otherRunningTask));
                        _logger.LogWarning($"Trigger cover block strategy by TaskId: {task.Id}, task failed", WriterTypes.Server, otherRunningTask.Id, otherRunningTask.JobId);
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

        if (task.TaskStatus != TaskRunStatus.WaitToRun)
        {
            task.Job.UpdateLastScheduleTime(task.SchedulerTime);
            task.Job.UpdateLastRunDetail(task.TaskStatus);
            await _schedulerJobRepository.UpdateAsync(task.Job);
        }

        await _schedulerTaskRepository.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();

        if (allowEnqueue)
        {
            await _serverManager.TaskEnqueue(task);
        }

        var dto = _mapper.Map<SchedulerTaskDto>(task);

        await _signalRUtils.SendNoticationByGroup(ConstStrings.GLOBAL_GROUP, SignalRMethodConsts.GET_NOTIFICATION, dto);
    }
}
