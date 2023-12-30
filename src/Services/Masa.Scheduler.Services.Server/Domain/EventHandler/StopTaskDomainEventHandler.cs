// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class StopTaskDomainEventHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly SchedulerServerManager _serverManager;
    private readonly SignalRUtils _signalRUtils;
    private readonly IMapper _mapper;
    private readonly SchedulerDbContext _dbContext;
    private readonly IDistributedCacheClient _distributedCacheClient;
    private readonly IIntegrationEventBus _eventBus;
    private readonly QuartzUtils _quartzUtils;

    public StopTaskDomainEventHandler(
        ISchedulerTaskRepository schedulerTaskRepository,
        IIntegrationEventBus eventBus,
        SchedulerServerManager serverManager,
        ISchedulerJobRepository schedulerJobRepository,
        SignalRUtils signalRUtils,
        IMapper mapper,
        SchedulerDbContext dbContext,
        IDistributedCacheClient distributedCacheClient,
        QuartzUtils quartzUtils)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _eventBus = eventBus;
        _serverManager = serverManager;
        _schedulerJobRepository = schedulerJobRepository;
        _signalRUtils = signalRUtils;
        _mapper = mapper;
        _dbContext = dbContext;
        _distributedCacheClient = distributedCacheClient;
        _quartzUtils = quartzUtils;
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
            if(task.TaskStatus == TaskRunStatus.WaitToRetry)
            {
                await _quartzUtils.RemoveDelayTask(task.Id, task.JobId);
            }

            task.TaskEnd(TaskRunStatus.Failure, $"User manual stop task, OperatorId: {@event.Request.OperatorId}");

            await _schedulerTaskRepository.UpdateAsync(task);

            var job = await _schedulerJobRepository.FindAsync(j => j.Id == task.JobId);

            if (job != null)
            {
                job.UpdateLastRunDetail(TaskRunStatus.Failure);
                await _schedulerJobRepository.UpdateAsync(job);
            }

            await _schedulerTaskRepository.UnitOfWork.SaveChangesAsync();
            await _schedulerTaskRepository.UnitOfWork.CommitAsync();

            var dto = _mapper.Map<SchedulerTaskDto>(task);

            var waitForRunTask = await _dbContext.Tasks.Include(t => t.Job).FirstOrDefaultAsync(t => t.TaskStatus == TaskRunStatus.WaitToRun && t.JobId == task.JobId);

            if (waitForRunTask != null)
            {
                var startWaittingTaskevent = new StartWaitingTaskIntergrationEvent()
                {
                    TaskId = waitForRunTask.Id,
                    OperatorId = task.OperatorId,
                };

                await _eventBus.PublishAsync(startWaittingTaskevent);
            }

            _distributedCacheClient.Remove($"{CacheKeys.TASK_RETRY_COUNT}_{task.Id}");

            await _signalRUtils.SendNoticationByGroup(ConstStrings.GLOBAL_GROUP, SignalRMethodConsts.GET_NOTIFICATION, dto);
        }
    }
}
