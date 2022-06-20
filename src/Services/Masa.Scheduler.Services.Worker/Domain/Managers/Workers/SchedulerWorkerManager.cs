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
        IHostApplicationLifetime hostApplicationLifetime, TaskHanlderFactory taskHandlerFactory)
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
            throw new UserFriendlyException("Job cannot be null");
        }

        _data.TaskQueue.Enqueue(new TaskRunModel() { Job = @event.Job, TaskId = @event.TaskId, ServiceId = @event.ServiceId });

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
                try
                {
                    if (string.IsNullOrWhiteSpace(_data.ServiceId))
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    if (_data.TaskQueue.Count == 0)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    if (_data.TaskQueue.TryDequeue(out var task))
                    {
                        if(task.ServiceId != _data.ServiceId)
                        {
                            Logger.LogWarning($"Get ServiceId: {task.ServiceId}, CurrentServiceId:{_data.ServiceId}");
                            continue;
                        }

                        if (_data.StopTask.Any(p => p == task.TaskId))
                        {
                            _data.StopTask.Remove(task.TaskId);
                            continue;
                        }

                        await StartTaskAsync(task.TaskId, task.Job);
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, "ProcessTaskRunError");
                }
            }
        });

        return Task.CompletedTask;
    }

    public async Task StartTaskAsync(Guid taskId, SchedulerJobDto job)
    {
        var cts = new CancellationTokenSource();
        var internalCts = new CancellationTokenSource();

        _data.TaskCancellationTokenSources.TryAdd(taskId, cts);

        _data.InternalCancellationTokenSources.TryAdd(taskId, internalCts);

        await NofityTaskStart(taskId);

        var taskHandler = _taskHandlerFactory.GetTaskHandler(job.JobType);

        var startTime = DateTime.Now;

        cts.Token.Register(async () =>
        {
            _data.TaskCancellationTokenSources.Remove(taskId, out var cancellationTokenSource);

            cancellationTokenSource?.Dispose();

            _data.InternalCancellationTokenSources.Remove(taskId, out var internalCancellationToken);

            if ((DateTime.Now - startTime).TotalSeconds >= job.RunTimeoutSecond && job.RunTimeoutStrategy == RunTimeoutStrategyTypes.IgnoreTimeout)
            {
                await NotifyTaskRunResult(TaskRunStatus.Timeout, taskId);
            }
            else
            {
                internalCancellationToken?.Cancel();
                internalCancellationToken?.Dispose();
                await NotifyTaskRunResult(TaskRunStatus.Failure, taskId);
            }
        });

        cts.CancelAfter(TimeSpan.FromSeconds(job.RunTimeoutSecond));

        _ = Task.Run(async () =>
        {
            try
            {
                var runStatus = await taskHandler.RunTask(taskId, job, internalCts.Token);

                await NotifyTaskRunResult(runStatus, taskId);
            }
            catch
            {
                await NotifyTaskRunResult(TaskRunStatus.Failure, taskId);
                throw;
            }
            finally
            {
                cts?.Dispose();
                internalCts?.Dispose();

                _data.TaskCancellationTokenSources.Remove(taskId, out _);
                _data.InternalCancellationTokenSources.Remove(taskId, out _);
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

        await EventBus.PublishAsync(@event);
        await EventBus.CommitAsync();
    }

    private async Task NofityTaskStart(Guid taskId)
    {
        var @event = new NotifyTaskStartIntegrationEvent()
        {
            TaskId = taskId
        };

        await EventBus.PublishAsync(@event);
        await EventBus.CommitAsync();
    }
}
