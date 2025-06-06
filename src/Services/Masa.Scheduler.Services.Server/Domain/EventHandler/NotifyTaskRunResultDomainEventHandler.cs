﻿// Copyright (c) MASA Stack All rights reserved.
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
    private readonly IMapper _mapper;
    private readonly SchedulerLogger _schedulerLogger;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;
    private readonly IDatabase _redis;

    public NotifyTaskRunResultDomainEventHandler(
        IRepository<SchedulerTask> schedulerTaskRepository,
        SchedulerDbContext dbContext,
        IRepository<SchedulerJob> schedulerJobRepository,
        SignalRUtils signalRUtils,
        IDistributedCacheClient distributedCacheClient,
        QuartzUtils quartzUtils,
        IIntegrationEventBus eventBus,
        SchedulerServerManagerData data,
        IMapper mapper,
        SchedulerLogger schedulerLogger,
        IMultiEnvironmentContext multiEnvironmentContext,
        ConnectionMultiplexer connect)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _dbContext = dbContext;
        _schedulerJobRepository = schedulerJobRepository;
        _signalRUtils = signalRUtils;
        _distributedCacheClient = distributedCacheClient;
        _quartzUtils = quartzUtils;
        _eventBus = eventBus;
        _data = data;
        _mapper = mapper;
        _schedulerLogger = schedulerLogger;
        _multiEnvironmentContext = multiEnvironmentContext;
        _redis = connect.GetDatabase();
    }

    [EventHandler]
    public async Task NotifyTaskRunResultAsync(NotifyTaskRunResultDomainEvent @event)
    {
        var task = await _dbContext.Tasks.Include(t => t.Job).FirstOrDefaultAsync(t => t.Id == @event.Request.TaskId);

        if (task == null)
        {
            throw new UserFriendlyException($"cannot find task, task Id: {@event.Request.TaskId}");
        }

        var stopByManualKey = ConstStrings.STOP_BY_MANUAL_KEY;

        if (await _redis.SetContainsAsync(stopByManualKey, @event.Request.TaskId.ToString()))
        {
            await _redis.SetRemoveAsync(stopByManualKey, @event.Request.TaskId.ToString());
            return;
        }

        LogTaskResult(@event.Request.Status, task.TraceId, task.Id, task.JobId);

        TaskRunStatus status = @event.Request.Status;

        if(!@event.Request.StopManaul && status == TaskRunStatus.Failure && task.Job.FailedStrategy == FailedStrategyTypes.Auto && task.Job.RunTimeoutStrategy == RunTimeoutStrategyTypes.RunFailedStrategy)
        {
            var retryCount = await _distributedCacheClient.HashIncrementAsync($"{CacheKeys.TASK_RETRY_COUNT}_{task.Id}");

            if(retryCount <= task.Job.FailedRetryCount)
            {
                status = TaskRunStatus.WaitToRetry;

                await _quartzUtils.AddDelayTask<StartSchedulerTaskQuartzJob>(_multiEnvironmentContext.CurrentEnvironment, task.Id, task.Job.Id, TimeSpan.FromSeconds(task.Job.FailedRetryInterval));
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
                TaskRunStatus.Ignore => "Ignore Task",
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

        task.SetTraceId(@event.Request.TraceId);

        await _schedulerJobRepository.UpdateAsync(task.Job);

        await _schedulerTaskRepository.UpdateAsync(task);

        if (task.TaskStatus != TaskRunStatus.WaitToRetry)
        {
            var waitForRunTask = await _dbContext.Tasks.AsNoTracking().Include(t => t.Job).OrderBy(x=>x.Id).FirstOrDefaultAsync(t => t.TaskStatus == TaskRunStatus.WaitToRun && t.JobId == task.JobId);

            if (waitForRunTask != null)
            {
                var startWaittingTaskevent = new StartWaitingTaskIntergrationEvent()
                {
                    TaskId = waitForRunTask.Id,
                    OperatorId = task.OperatorId,
                };

                await _eventBus.PublishAsync(startWaittingTaskevent);
            }

            await _distributedCacheClient.RemoveAsync<long>($"{CacheKeys.TASK_RETRY_COUNT}_{task.Id}");
        }

        var dto = _mapper.Map<SchedulerTaskDto>(task);

        await _signalRUtils.SendNoticationByGroup(ConstStrings.GLOBAL_GROUP, SignalRMethodConsts.GET_NOTIFICATION, dto);
    }

    private void LogTaskResult(TaskRunStatus status, string traceId, Guid taskId, Guid jobId)
    {
        var logMessage = $"Receive notify task result, status: {status}，TraceId:{traceId}";

        if (status == TaskRunStatus.Failure)
        {
            _schedulerLogger.LogError(logMessage, WriterTypes.Server, taskId, jobId);
        }
        else
        {
            _schedulerLogger.LogInformation(logMessage, WriterTypes.Server, taskId, jobId);
        }
    }
}
