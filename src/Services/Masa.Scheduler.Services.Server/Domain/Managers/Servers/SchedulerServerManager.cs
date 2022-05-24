// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Dapr.Client;
using Masa.Scheduler.Contracts.Server.Infrastructure.Managers;

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class SchedulerServerManager : BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>
{
    private readonly ILogger<SchedulerServerManager> _logger;
    private readonly IMapper _mapper;

    public SchedulerServerManager(IDistributedCacheClientFactory cacheClientFactory, IDistributedCacheClient redisCacheClient, IServiceProvider serviceProvider, IIntegrationEventBus eventBus, ILogger<SchedulerServerManager> logger, IHttpClientFactory httpClientFactory, IMapper mapper) : base(cacheClientFactory, redisCacheClient, serviceProvider, eventBus, httpClientFactory)
    {
        _logger = logger;
        _mapper = mapper;
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_SERVER_MANAGER_API}/heartbeat";

    protected override ILogger<BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>> Logger => _logger;

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

    public async Task StartTask(SchedulerTask task, WorkerModel worker)
    {
        var taskDto = _mapper.Map<SchedulerTaskDto>(task);

        var startTaskRequest = new StartTaskRequest() { TaskId = taskDto.Id, Job = taskDto.Job };

        var response = await worker.CallerClient.PostAsync(ConstStrings.SCHEDULER_WORKER_MANAGER_API + "/StartTask", JsonContent.Create(startTaskRequest));

        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"Worker: {worker.GetServiceUrl()} not response");
        }
    }

    public async Task StopTask(Guid taskId, string workerHost)
    {
        var worker = await GetWorker(workerHost);

        var stopTaskRequest = new StopTaskRequest() { TaskId = taskId };

        var response = await worker.CallerClient.PostAsync(ConstStrings.SCHEDULER_WORKER_MANAGER_API + "/StopTask", JsonContent.Create(stopTaskRequest));

        if (!response.IsSuccessStatusCode)
        {
            throw new UserFriendlyException($"Worker: {worker.GetServiceUrl} not response");
        }
    }

    private static void CheckWorkerNotNull(WorkerModel? worker)
    {
        if (worker is null)
        {
            throw new UserFriendlyException("Worker cannot be null");
        }
    }

}
