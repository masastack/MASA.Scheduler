// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Managers.Workers;

public class SchedulerWorkerManager : BaseSchedulerManager<ServerModel, SchedulerWorkerOnlineIntegrationEvent, SchedulerServerOnlineIntegrationEvent>, IDisposable
{
    private ILogger<SchedulerWorkerManager> _logger;

    private readonly TaskHanlderFactory _taskHandlerFactory;

    private readonly SchedulerLogger _schedulerLogger;
    private readonly SchedulerWorkerManagerData _schedulerWorkerManagerData;

    public SchedulerWorkerManager(IDistributedCacheClientFactory cacheClientFactory,
        IDistributedCacheClient redisCacheClient,
        IServiceProvider serviceProvider,
        IIntegrationEventBus eventBus,
        ILogger<SchedulerWorkerManager> logger,
        IHttpClientFactory httpClientFactory,
        SchedulerWorkerManagerData data,
        IHostApplicationLifetime hostApplicationLifetime,
        TaskHanlderFactory taskHandlerFactory,
        SchedulerLogger schedulerLogger)
        : base(
            cacheClientFactory,
            redisCacheClient,
            serviceProvider,
            eventBus,
            httpClientFactory,
            data,
            hostApplicationLifetime)
    {
        _logger = logger;
        _taskHandlerFactory = taskHandlerFactory;
        _schedulerLogger = schedulerLogger;
        _schedulerWorkerManagerData = data;
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_WORKER_MANAGER_API}/heartbeat";

    protected override string OnlineApi { get; set; } = $"{ConstStrings.SCHEDULER_WORKER_MANAGER_API}/online";

    protected override string OnlineTopic { get; set; } = $"{nameof(SchedulerWorkerOnlineIntegrationEvent)}";

    protected override string MoniterTopic { get; set; } = $"{nameof(SchedulerServerOnlineIntegrationEvent)}";

    protected override ILogger<BaseSchedulerManager<ServerModel, SchedulerWorkerOnlineIntegrationEvent, SchedulerServerOnlineIntegrationEvent>> Logger => _logger;

    public async Task EnqueueTask(StartTaskIntegrationEvent @event)
    {
        if (@event.Job is null)
        {
            _logger.LogError($"SchedulerWorkerManager: Job is null, TaskId: {@event.TaskId}");
            throw new UserFriendlyException("Job cannot be null");
        }

        _schedulerLogger.LogInformation($"Task Enqueue", WriterTypes.Worker, @event.TaskId, @event.Job.Id);
        _schedulerWorkerManagerData.TaskQueue.Enqueue(new TaskRunModel() { Job = @event.Job, TaskId = @event.TaskId, ServiceId = @event.ServiceId, ExcuteTime = @event.ExcuteTime });
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
                var data = _schedulerWorkerManagerData;

                try
                {
                    if (string.IsNullOrWhiteSpace(data.ServiceId))
                    {
                        _logger.LogInformation($"SchedulerWorkerManager: ServiceId is null");
                        await Task.Delay(1000);
                        continue;
                    }

                    if (data.TaskQueue.Count == 0)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    if (data.TaskQueue.TryDequeue(out var task))
                    {
                        _schedulerLogger.LogInformation($"Task Dequeue", WriterTypes.Worker, task.TaskId, task.Job.Id);

                        if (task.ServiceId != data.ServiceId)
                        {
                            _schedulerLogger.LogInformation($"ServiceId not same, Task serviceId: {task.ServiceId}, CurrentWorkerServiceId: {data.ServiceId}", WriterTypes.Worker, task.TaskId, task.Job.Id);
                            continue;
                        }

                        if (data.StopTask.Any(p => p == task.TaskId))
                        {
                            _schedulerLogger.LogInformation($"Task Stop", WriterTypes.Worker, task.TaskId, task.Job.Id);
                            data.StopTask.Remove(task.TaskId);
                            continue;
                        }

                        _schedulerLogger.LogInformation($"Worker ready to Start Task", WriterTypes.Worker, task.TaskId, task.Job.Id);
                        await StartTaskAsync(data, task.TaskId, task.Job, task.ExcuteTime);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ProcessTaskRunError");
                }
            }
        });

        return Task.CompletedTask;
    }

    private Task StartTaskAsync(SchedulerWorkerManagerData data, Guid taskId, SchedulerJobDto job, DateTimeOffset excuteTime)
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
                _schedulerLogger.LogInformation($"Task Cancel", WriterTypes.Worker, taskId, job.Id);

                var data = _schedulerWorkerManagerData;

                if (!data.StopTask.Contains(taskId) && job.RunTimeoutSecond > 0 && (DateTime.Now - startTime).TotalSeconds >= job.RunTimeoutSecond && job.RunTimeoutStrategy == RunTimeoutStrategyTypes.IgnoreTimeout)
                {
                    await NotifyTaskRunResult(TaskRunStatus.Timeout, taskId, job.Id);
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

        if (job.RunTimeoutSecond > 0)
        {
            cts.CancelAfter(TimeSpan.FromSeconds(job.RunTimeoutSecond));
        }

        _ = Task.Run(async () =>
        {
            //await using var scope = ServiceProvider.CreateAsyncScope();
            //var provider = scope.ServiceProvider;
            //var managerData = provider.GetRequiredService<SchedulerWorkerManagerData>();
            var managerData = _schedulerWorkerManagerData;

            try
            {
                _schedulerLogger.LogInformation($"Task run", WriterTypes.Worker, taskId, job.Id);
                var runStatus = await taskHandler.RunTask(taskId, job, excuteTime, internalCts.Token);
                await NotifyTaskRunResult(runStatus, taskId, job.Id);
            }
            catch (Exception ex)
            {
                await NotifyTaskRunResult(TaskRunStatus.Failure, taskId, job.Id, ex.Message);
                _schedulerLogger.LogError(ex, $"TaskHandler RunTask Error, Exception message: {ex.Message}", WriterTypes.Worker, taskId, job.Id);
            }
            finally
            {
                cts?.Dispose();
                internalCts?.Dispose();

                managerData.TaskCancellationTokenSources.Remove(taskId, out _);
                managerData.InternalCancellationTokenSources.Remove(taskId, out _);
            }
        }, cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopTaskAsync(StopTaskIntegrationEvent @event)
    {
        var data = _schedulerWorkerManagerData;

        if (@event.ServiceId == data.ServiceId)
        {
            if (data.TaskCancellationTokenSources.TryGetValue(@event.TaskId, out var task))
            {
                data.StopTask.Add(@event.TaskId);
                task.Cancel();
            }
            else if (data.TaskQueue.Any(t => t.TaskId == @event.TaskId))
            {
                data.StopTask.Add(@event.TaskId);
            }
        }
    }

    private async Task NotifyTaskRunResult(TaskRunStatus runStatus, Guid taskId, Guid jobId, string message = "")
    {
        var @event = new NotifyTaskRunResultIntegrationEvent()
        {
            TaskId = taskId,
            Status = runStatus,
            Message = message
        };

        _schedulerLogger.LogInformation($"Task notify run result, Status: {runStatus.ToString()}", WriterTypes.Worker, taskId, jobId);

        await using var scope = ServiceProvider.CreateAsyncScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IIntegrationEventBus>();
        await eventBus.PublishAsync(@event);
    }

    public void Dispose()
    {
        Console.WriteLine("客户端被释放");
    }
}
