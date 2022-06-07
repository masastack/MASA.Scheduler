// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class SchedulerServerManager : BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>
{
    private readonly ILogger<SchedulerServerManager> _logger;
    private readonly IMapper _mapper;
    private readonly SchedulerServerManagerData _data;
    private readonly IRepository<SchedulerTask> _repository;

    public SchedulerServerManager(
        IDistributedCacheClientFactory cacheClientFactory,
        IDistributedCacheClient redisCacheClient,
        IServiceProvider serviceProvider,
        IIntegrationEventBus eventBus,
        ILogger<SchedulerServerManager> logger,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        SchedulerServerManagerData data, IRepository<SchedulerTask> repository)
        : base(cacheClientFactory, redisCacheClient, serviceProvider, eventBus, httpClientFactory, data)
    {
        _logger = logger;
        _mapper = mapper;
        _data = data;
        _repository = repository;
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
                var currentUesIndex = Convert.ToInt32((currentRunCount - 1) % ServiceList.FindAll(w => w.Status == ServiceStatus.Normal).Count);
                worker = ServiceList[currentUesIndex];
                break;
            case RoutingStrategyTypes.DynamicRatioApm:
                break;
        }

        CheckWorkerNotNull(worker);

        return worker!;
    }

    public Task<WorkerModel?> GetWorker(string workerHost)
    {
        WorkerModel? workerModel = null;

        if (string.IsNullOrEmpty(workerHost))
        {
            return Task.FromResult(workerModel);
        }

        var uri = new Uri(workerHost);
       
        if(uri.Scheme == "http")
        {
            workerModel = ServiceList.FirstOrDefault(w => w.HttpHost == uri.Host && w.HttpPort == uri.Port && w.Status == ServiceStatus.Normal);
        }
        else
        {
            workerModel = ServiceList.FirstOrDefault(w => w.HttpsHost == uri.Host && w.HttpsPort == uri.Port && w.Status == ServiceStatus.Normal);
        }
        
        return Task.FromResult(workerModel);
    }

    public Task TaskEnqueue(SchedulerTask task)
    {
        var taskDto = _mapper.Map<SchedulerTaskDto>(task);

        _data.TaskQueue.Enqueue(taskDto);

        return Task.CompletedTask;
    }

    public async Task StartTask(SchedulerTaskDto taskDto, WorkerModel worker)
    {
        var @event = new StartTaskIntegrationEvent()
        {
            TaskId = taskDto.Id,
            Job = taskDto.Job,
            ServiceId = worker.ServiceId
        };

        await EventBus.PublishAsync(@event);
        await EventBus.CommitAsync();
    }

    public async Task StopTask(Guid taskId, string workerHost)
    {
        if(_data.TaskQueue.Any(p => p.Id == taskId))
        {
            _data.StopTask.Add(taskId);
        }
        else
        {
            var worker = await GetWorker(workerHost);

            if(worker != null)
            {
                var @event = new StopTaskIntegrationEvent()
                {
                    TaskId = taskId,
                    ServiceId = worker.ServiceId
                };

                await EventBus.PublishAsync(@event);
                await EventBus.CommitAsync();
            }
        }
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

                if(!_data.ServiceList.Any())
                {
                    await Task.Delay(1000);
                    continue;
                }

                try
                {
                    if (_data.TaskQueue.TryDequeue(out var taskDto))
                    {
                        if (_data.StopTask.Any(p => p == taskDto.Id))
                        {
                            _data.StopTask.Remove(taskDto.Id);
                            continue;
                        }

                        WorkerModel? workerModel;

                        if (taskDto.Job.RoutingStrategy == RoutingStrategyTypes.Specified)
                        {
                            workerModel = await GetWorker(taskDto.Job.SpecifiedWorkerHost);

                            if(workerModel == null)
                            {
                                Logger.LogError($"Cannot find worker model, workerHost: {taskDto.Job.SpecifiedWorkerHost}");
                                continue;
                            }
                        }
                        else
                        {
                            workerModel = await GetWorker(taskDto.Job.RoutingStrategy);
                        }

                        await CheckHeartbeat(workerModel!);

                        if(workerModel.Status != ServiceStatus.Normal)
                        {
                            _data.TaskQueue.Enqueue(taskDto);
                            await Task.Delay(1000);
                        }

                        var task = await _repository.FindAsync(p=> p.Id == taskDto.Id);

                        if(task == null)
                        {
                            throw new UserFriendlyException($"Task is not found, taskId: {taskDto.Id}");
                        }

                        task.SetWorkerHost(workerModel.GetServiceUrl());

                        await _repository.UpdateAsync(task);

                        await _repository.UnitOfWork.SaveChangesAsync();

                        await StartTask(taskDto, workerModel);
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
