// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class NotifyTaskRunResultDomainEventHandler
{
    private readonly IRepository<SchedulerTask> _schedulerTaskRepository;
    private readonly SchedulerDbContext _dbContext;
    private readonly IRepository<SchedulerJob> _schedulerJobRepository;
    private readonly SignalRUtils _signalRUtils;
    private readonly IDistributedCacheClient _distributedCacheClient;
    private readonly QuartzUtils _quartzUtils;
    private readonly IIntegrationEventBus _eventBus;
    private readonly SchedulerServerManagerData _data;

    public NotifyTaskRunResultDomainEventHandler(
        IRepository<SchedulerTask> schedulerTaskRepository,
        SchedulerDbContext dbContext,
        IRepository<SchedulerJob> schedulerJobRepository,
        SignalRUtils signalRUtils,
        IDistributedCacheClient distributedCacheClient,
        QuartzUtils quartzUtils,
        IIntegrationEventBus eventBus,
        SchedulerServerManagerData data)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _dbContext = dbContext;
        _schedulerJobRepository = schedulerJobRepository;
        _signalRUtils = signalRUtils;
        _distributedCacheClient = distributedCacheClient;
        _quartzUtils = quartzUtils;
        _eventBus = eventBus;
        _data = data;
    }

    [EventHandler]
    public async Task NotifyTaskRunResultAsync(NotifyTaskRunResultDomainEvent @event)
    {
        var task = await _dbContext.Tasks.Include(t => t.Job).FirstOrDefaultAsync(t => t.Id == @event.Request.TaskId);

        if (task == null)
        {
            throw new UserFriendlyException($"cannot find task, task Id: {@event.Request.TaskId}");
        }

        if (_data.StopByManual.Contains(@event.Request.TaskId))
        {
            _data.StopByManual.Remove(@event.Request.TaskId);
            return;
        }

        TaskRunStatus status = @event.Request.Status;

        if(status == TaskRunStatus.Failure && task.Job.FailedStrategy == FailedStrategyTypes.Auto)
        {
            var retryCount = await _distributedCacheClient.HashIncrementAsync($"{CacheKeys.TASK_RETRY_COUNT}_{task.Id}");

            if(retryCount <= task.Job.FailedRetryCount)
            {
                status = TaskRunStatus.WaitToRetry;

                await _quartzUtils.AddDelayTask<StartSchedulerTaskQuartzJob>(task.Id, task.Job.Id, TimeSpan.FromSeconds(task.Job.FailedRetryInterval));
            }
        }

        string? message = @event.Request.Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            message = status switch
            {
                //todo: i18n
                TaskRunStatus.Success => "Task run success",
                TaskRunStatus.Timeout => "Task run timeout",
                TaskRunStatus.Failure => "Task run failure",
                TaskRunStatus.WaitToRetry => "Wait for auto retry",
                _ => ""
            };
        }

        if (task.TaskStatus == TaskRunStatus.Timeout && status == TaskRunStatus.Success)
        {
            status = TaskRunStatus.TimeoutSuccess;

            message = "Task run success, but timeout";
        }

        task.TaskEnd(status, message);

        task.Job.UpdateLastRunDetail(status);

        await _schedulerJobRepository.UpdateAsync(task.Job);

        await _schedulerTaskRepository.UpdateAsync(task);

        await _schedulerJobRepository.UnitOfWork.SaveChangesAsync();

        await _schedulerJobRepository.UnitOfWork.CommitAsync();

        if (task.TaskStatus != TaskRunStatus.WaitToRetry)
        {
            var waitForRunTask = await _dbContext.Tasks.OrderBy(t => t.SchedulerTime).ThenBy(t => t.CreationTime).Include(t => t.Job).FirstOrDefaultAsync(t => t.TaskStatus == TaskRunStatus.WaitToRun && t.JobId == task.JobId);

            if (waitForRunTask != null)
            {
                var startWaittingTaskevent = new StartWaitingTaskIntergrationEvent()
                {
                    TaskId = waitForRunTask.Id,
                    OperatorId = task.OperatorId,
                };

                await _eventBus.PublishAsync(startWaittingTaskevent);
            }

            _distributedCacheClient.Remove<int>($"{CacheKeys.TASK_RETRY_COUNT}_{task.Id}");
        }

        await _signalRUtils.SendNoticationByGroup(ConstStrings.GLOBAL_GROUP, SignalRMethodConsts.GET_NOTIFICATION);
    }
}
