// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Managers.Workers;

public class SchedulerWorkerManager : BaseSchedulerManager<ServerModel, SchedulerWorkerOnlineIntegrationEvent, SchedulerServerOnlineIntegrationEvent>
{
    private ILogger<SchedulerWorkerManager> _logger;

    private readonly SchedulerWorkerManagerData _data;

    private readonly TaskHanlderFactory _taskHandlerFactory;

    public SchedulerWorkerManager(IDistributedCacheClientFactory cacheClientFactory,
        IDistributedCacheClient redisCacheClient,
        IServiceProvider serviceProvider,
        IIntegrationEventBus eventBus,
        ILogger<SchedulerWorkerManager> logger,
        IHttpClientFactory httpClientFactory,
        SchedulerWorkerManagerData data,
        IHostApplicationLifetime hostApplicationLifetime, 
        TaskHanlderFactory taskHandlerFactory)
        : base(cacheClientFactory, redisCacheClient, serviceProvider, eventBus, httpClientFactory, data, hostApplicationLifetime)
    {
        _data = data;
        _logger = logger;
        _taskHandlerFactory = taskHandlerFactory;
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_WORKER_MANAGER_API}/heartbeat";

    protected override ILogger<BaseSchedulerManager<ServerModel, SchedulerWorkerOnlineIntegrationEvent, SchedulerServerOnlineIntegrationEvent>> Logger => _logger;
    
    public Task EnqueueTask(StartTaskIntegrationEvent @event)
    {
        if (@event.Job is null)
        {
            _logger.LogError($"SchedulerWorkerManager: Job is null, TaskId: {@event.TaskId}");
            throw new UserFriendlyException("Job cannot be null");
        }

        _logger.LogInformation($"SchedulerWorkerManager: Task Enqueue, TaskId: {@event.TaskId}, JobId: {@event.Job.Id}");
        _data.TaskQueue.Enqueue(new TaskRunModel() { Job = @event.Job, TaskId = @event.TaskId, ServiceId = @event.ServiceId, ExcuteTime = @event.ExcuteTime });

        return Task.CompletedTask;
    }

    public override async Task OnManagerStartAsync()
    {
        try
        {
            await base.OnManagerStartAsync();

            await ProcessTaskRun();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "OnManagerStartAsync");
        }
    }

    private Task ProcessTaskRun()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await using var scope = ServiceProvider.CreateAsyncScope();

                var provider = scope.ServiceProvider;

                var data = provider.GetRequiredService<SchedulerWorkerManagerData>();

                try
                {
                    if (string.IsNullOrWhiteSpace(data.ServiceId))
                    {
                        _logger.LogInformation($"SchedulerWorkerManager: ServiceId is null");
                        await Task.Delay(100);
                        continue;
                    }

                    if (data.TaskQueue.Count == 0)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    if (data.TaskQueue.TryDequeue(out var task))
                    {
                        _logger.LogInformation($"SchedulerWorkerManager: Task Dequeue, TaskId: {task.TaskId}, JobId: {task.Job.Id}");

                        if (task.ServiceId != data.ServiceId)
                        {
                            _logger.LogInformation($"SchedulerWorkerManager: ServiceId not same, Get ServiceId: {task.ServiceId}, CurrentServiceId:{data.ServiceId}, TaskId: {task.TaskId}, JobId: {task.Job.Id}");
                            continue;
                        }

                        if (data.StopTask.Any(p => p == task.TaskId))
                        {
                            _logger.LogInformation($"SchedulerWorkerManager: Task Stop, TaskId: {task.TaskId}, JobId: {task.Job.Id}");
                            data.StopTask.Remove(task.TaskId);
                            continue;
                        }

                        await StartTaskAsync(data, task.TaskId, task.Job, task.ExcuteTime);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "ProcessTaskRunError");
                }
            }
        });

        return Task.CompletedTask;
    }

    public async Task StartTaskAsync(SchedulerWorkerManagerData data, Guid taskId, SchedulerJobDto job, DateTimeOffset excuteTime)
    {
        var cts = new CancellationTokenSource();
        var internalCts = new CancellationTokenSource();

        data.TaskCancellationTokenSources.TryAdd(taskId, cts);

        data.InternalCancellationTokenSources.TryAdd(taskId, internalCts);

        var taskHandler = _taskHandlerFactory.GetTaskHandler(job.JobType);

        var startTime = DateTime.Now;

        cts.Token.Register(async () =>
        {
            try
            {
                _logger.LogInformation($"SchedulerWorkerManager: Task Cancel, TaskId: {taskId}, JobId: {job.Id}");

                if (job.RunTimeoutSecond > 0 && (DateTime.Now - startTime).TotalSeconds >= job.RunTimeoutSecond && job.RunTimeoutStrategy == RunTimeoutStrategyTypes.IgnoreTimeout)
                {
                    await NotifyTaskRunResult(TaskRunStatus.Timeout, taskId);
                }
                else
                {
                    data.InternalCancellationTokenSources.TryGetValue(taskId, out var internalCancellationToken);
                    internalCancellationToken?.Cancel();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TokenRegisterError");
            }
        });

        if(job.RunTimeoutSecond > 0)
        {
            cts.CancelAfter(TimeSpan.FromSeconds(job.RunTimeoutSecond));
        }

        _ = Task.Run(async () =>
        {
            await using var scope = ServiceProvider.CreateAsyncScope();
            var provider = scope.ServiceProvider;
            var managerData = provider.GetRequiredService<SchedulerWorkerManagerData>();

            try
            {
                _logger.LogInformation($"SchedulerWorkerManager: Task run, TaskId: {taskId}, JobId: {job.Id}");
                var runStatus = await taskHandler.RunTask(taskId, job, excuteTime, internalCts.Token);
                await NotifyTaskRunResult(runStatus, taskId);
            }
            catch (Exception ex)
            {
                await NotifyTaskRunResult(TaskRunStatus.Failure, taskId);
                _logger.LogError(ex, "TaskHandler RunTask Error");
            }
            finally
            {
                cts?.Dispose();
                internalCts?.Dispose();

                managerData.TaskCancellationTokenSources.Remove(taskId, out _);
                managerData.InternalCancellationTokenSources.Remove(taskId, out _);
            }
        }, cts.Token);
    }

    public Task StopTaskAsync(StopTaskIntegrationEvent @event)
    {
        if(@event.ServiceId == _data.ServiceId)
        {
            if (_data.TaskCancellationTokenSources.TryGetValue(@event.TaskId, out var task))
            {
                task.Cancel();
            }
            else if(_data.TaskQueue.Any(t=> t.TaskId == @event.TaskId))
            {
                _data.StopTask.Add(@event.TaskId);
            }
        }

        return Task.CompletedTask;
    }

    private async Task NotifyTaskRunResult(TaskRunStatus runStatus, Guid taskId)
    {
        var @event = new NotifyTaskRunResultIntegrationEvent()
        {
            TaskId = taskId,
            Status = runStatus
        };

        _logger.LogInformation($"SchedulerWorkerManager: Task notify run result, TaskId: {taskId}, Status: {runStatus.ToString()}");

        await using var scope = ServiceProvider.CreateAsyncScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IIntegrationEventBus>();
        await eventBus.PublishAsync(@event);
        await eventBus.CommitAsync();
    }
}
