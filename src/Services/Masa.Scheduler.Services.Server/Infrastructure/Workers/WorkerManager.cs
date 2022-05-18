// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Workers;

public class WorkerManager
{
    private List<WorkerModel> _workerList = new();
    private IDistributedCacheClientFactory _cacheClientFactory;
    private IDistributedCacheClient _redisCacheClient;
    public WorkerManager(IDistributedCacheClientFactory cacheClientFactory)
    {
        _cacheClientFactory = cacheClientFactory;
        _redisCacheClient = _cacheClientFactory.CreateClient(string.Empty);

        _workerList.Add(new WorkerModel()
        {
             Host = "localhost",
             Port = 8899,
             IsRunning = true
        });
    }

    public async Task<WorkerModel> GetWorker(RoutingStrategyTypes routingType)
    {
        WorkerModel? worker = null;

        switch (routingType)
        {
            case RoutingStrategyTypes.RoundRobin:
                var currentRunCount = await _redisCacheClient.HashIncrementAsync(CacheKeys.CURRENT_RUN_COUNT);
                var currentUesIndex = Convert.ToInt32((currentRunCount - 1) % _workerList.FindAll(w => w.IsRunning).Count);
                worker = _workerList[currentUesIndex];
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

        if(hostInfo.Length != 2)
        {
            throw new UserFriendlyException("WorkerHost Error");
        }

        var host = hostInfo[0];

        var port = Convert.ToInt32(hostInfo[1]);

        var workerModel = _workerList.FirstOrDefault(w => w.Host == host && w.Port == port && w.IsRunning);

        CheckWorkerNotNull(workerModel);

        return Task.FromResult(workerModel!);
    }

    public Task StartTask(SchedulerTask task, WorkerModel worker)
    {
        return Task.CompletedTask;
    }

    public async Task StopTask(Guid taskId, string workerHost)
    {
        var worker = await GetWorker(workerHost);
    }

    private void CheckWorkerNotNull(WorkerModel? worker)
    {
        if (worker is null)
        {
            throw new UserFriendlyException("Worker cannot be null");
        }
    }

}
