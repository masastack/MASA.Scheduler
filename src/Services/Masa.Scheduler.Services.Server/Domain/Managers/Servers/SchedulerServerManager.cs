// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class SchedulerServerManager : BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>
{
    private readonly ILogger<SchedulerServerManager> _logger;
    private readonly IMapper _mapper;
    private readonly SchedulerServerManagerData _data;

    public SchedulerServerManager(
        IDistributedCacheClientFactory cacheClientFactory,
        IDistributedCacheClient redisCacheClient,
        IServiceProvider serviceProvider,
        IIntegrationEventBus eventBus,
        ILogger<SchedulerServerManager> logger,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        SchedulerServerManagerData data)
        : base(cacheClientFactory, redisCacheClient, serviceProvider, eventBus, httpClientFactory, data)
    {
        _logger = logger;
        _mapper = mapper;
        _data = data;
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_SERVER_MANAGER_API}/heartbeat";

    protected override ILogger<BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>> Logger => _logger;

    public override async Task OnManagerStartAsync()
    {
        await base.OnManagerStartAsync();

        await StartAssignAsync();
    }

    public async Task<WorkerModel> GetWorker(RoutingStrategyTypes routingType)
    {
        if (!ServiceList.Any())
        {
            throw new UserFriendlyException("WorkerList is empty");
        }

        WorkerModel? worker = null;

        switch (routingType)
        {
            case RoutingStrategyTypes.RoundRobin:
                var currentRunCount = await RedisCacheClient.HashIncrementAsync(CacheKeys.CURRENT_RUN_COUNT);
                var currentUesIndex = Convert.ToInt32((currentRunCount - 1) % ServiceList.FindAll(w => w.Status == ServiceStatuses.Normal).Count);
                worker = ServiceList[currentUesIndex];
                break;
            case RoutingStrategyTypes.DynamicRatioApm:
                break;
        }

        CheckWorkerNotNull(worker);

        return worker!;
    }

    public Task<WorkerModel> GetWorker(string workerHost)
    {
        if (string.IsNullOrEmpty(workerHost))
        {
            throw new UserFriendlyException("Worker host cannot empty");
        }

        var hostInfo = workerHost.Split(':');

        if (hostInfo.Length != 2)
        {
            throw new UserFriendlyException("WorkerHost Error");
        }

        var host = hostInfo[0];

        var port = Convert.ToInt32(hostInfo[1]);

        var workerModel = ServiceList.FirstOrDefault(w => w.HttpHost == host && w.HttpPort == port && w.Status == ServiceStatuses.Normal);

        CheckWorkerNotNull(workerModel);

        return Task.FromResult(workerModel!);
    }

    public Task TaskEnqueue(SchedulerTask task, WorkerModel worker)
    {
        var taskAssignModel = new TaskAssignModel()
        {
            Task = _mapper.Map<SchedulerTaskDto>(task),
            Worker = worker
        };

        _data.TaskQueue.Enqueue(taskAssignModel);

        return Task.CompletedTask;
    }

    public async Task StartTask(SchedulerTaskDto taskDto, WorkerModel worker)
    {
        var @event = new StartTaskIntegrationEvent()
        {
            TaskId = taskDto.Id,
            Job = taskDto.Job,
            ProgramId = worker.ProgramId
        };

        await EventBus.PublishAsync(@event);
        await EventBus.CommitAsync();
    }

    public async Task StopTask(Guid taskId, string workerHost)
    {
        var worker = await GetWorker(workerHost);

        var @event = new StopTaskIntegrationEvent()
        {
            TaskId = taskId,
            ProgramId = worker.ProgramId
        };

        await EventBus.PublishAsync(@event);
        await EventBus.CommitAsync();
    }

    private static void CheckWorkerNotNull(WorkerModel? worker)
    {
        if (worker is null)
        {
            throw new UserFriendlyException("Worker cannot be null");
        }
    }

    public Task StartAssignAsync()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                if (_data.TaskQueue.Count == 0)
                {
                    await Task.Delay(1000);
                    continue;
                }

                try
                {
                    if (_data.TaskQueue.TryDequeue(out var task))
                    {
                        await StartTask(task.Task, task.Worker);
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, "StartAssignAsync");
                }
            }
        });

        return Task.CompletedTask;
    }
}
