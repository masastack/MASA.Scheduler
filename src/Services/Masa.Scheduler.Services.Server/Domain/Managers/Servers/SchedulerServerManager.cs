// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class SchedulerServerManager : BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>
{
    private readonly ILogger<SchedulerServerManager> _logger;
    private readonly QuartzUtils _quartzUtils;
    private readonly IMapper _mapper;
    private readonly IRepository<SchedulerResource> _resourceRepository;
    private readonly SchedulerLogger _schedulerLogger;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;
    private readonly ISchedulerTaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SignalRUtils _signalRUtils;
    private readonly IDatabase _redis;

    public SchedulerServerManager(
        IDistributedCacheClientFactory cacheClientFactory,
        IDistributedCacheClient redisCacheClient,
        IServiceProvider serviceProvider,
        IIntegrationEventBus eventBus,
        ILogger<SchedulerServerManager> logger,
        IHttpClientFactory httpClientFactory,
        SchedulerServerManagerData data,
        IHostApplicationLifetime hostApplicationLifetime,
        QuartzUtils quartzUtils,
        IMasaStackConfig masaStackConfig,
        IMapper mapper,
        IRepository<SchedulerResource> resourceRepository,
        SchedulerLogger schedulerLogger,
        IMultiEnvironmentContext multiEnvironmentContext,
        ISchedulerTaskRepository repository,
        IUnitOfWork unitOfWork,
        SignalRUtils signalRUtils,
        ConnectionMultiplexer connect)
        : base(cacheClientFactory,
               redisCacheClient,
               serviceProvider,
               eventBus,
               httpClientFactory,
               data,
               hostApplicationLifetime,
               masaStackConfig)
    {
        _logger = logger;
        _quartzUtils = quartzUtils;
        _mapper = mapper;
        _resourceRepository = resourceRepository;
        _schedulerLogger = schedulerLogger;
        _multiEnvironmentContext = multiEnvironmentContext;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _signalRUtils = signalRUtils;
        _redis = connect.GetDatabase();
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_SERVER_MANAGER_API}/heartbeat";

    protected override string OnlineApi { get; set; } = $"{ConstStrings.SCHEDULER_SERVER_MANAGER_API}/online";

    protected override string OnlineTopic { get; set; } = $"{nameof(SchedulerServerOnlineIntegrationEvent)}";

    protected override string MoniterTopic { get; set; } = $"{nameof(SchedulerWorkerOnlineIntegrationEvent)}";

    protected override ILogger<BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>> Logger => _logger;

    public override async Task OnManagerStartAsync()
    {
        await base.OnManagerStartAsync();

        await _quartzUtils.StartQuartzScheduler();
    }

    public async Task<WorkerModel?> GetWorker(SchedulerServerManagerData data, RoutingStrategyTypes routingType)
    {
        if (!data.ServiceList.FindAll(w => w.Status == ServiceStatus.Normal).Any())
        {
            return null;
        }

        WorkerModel? worker = null;

        switch (routingType)
        {
            case RoutingStrategyTypes.RoundRobin:
                var currentRunCount = await RedisCacheClient.HashIncrementAsync(CacheKeys.CURRENT_RUN_COUNT);
                var serviceCount = data.ServiceList.FindAll(w => w.Status == ServiceStatus.Normal).Count;
                var currentUesIndex = Convert.ToInt32((currentRunCount - 1) % serviceCount);
                worker = data.ServiceList[currentUesIndex];
                break;
                //case RoutingStrategyTypes.DynamicRatioApm:
                //    break;
        }
        return worker!;
    }

    public Task<WorkerModel?> GetWorker(SchedulerServerManagerData data, string workerHost)
    {
        WorkerModel? workerModel = null;

        if (string.IsNullOrEmpty(workerHost))
        {
            return Task.FromResult(workerModel);
        }

        var uri = new Uri(workerHost);

        if (uri.Scheme == Uri.UriSchemeHttp)
        {
            workerModel = data.ServiceList.FirstOrDefault(w => w.HttpServiceUrl == workerHost && w.Status == ServiceStatus.Normal);
        }
        else
        {
            workerModel = data.ServiceList.FirstOrDefault(w => w.HttpsServiceUrl == workerHost && w.Status == ServiceStatus.Normal);
        }

        return Task.FromResult(workerModel);
    }

    public async Task TaskEnqueue(SchedulerTask task)
    {
        _schedulerLogger.LogInformation($"Task Enqueue", WriterTypes.Server, task.Id, task.JobId);

        await EnqueueTaskAsync(task.Id);
    }

    public async Task StopTask(Guid taskId, string workerHost)
    {
        var data = ServiceProvider.GetRequiredService<SchedulerServerManagerData>();

        if(await ExistsTaskAsync(taskId))
        {
            await AddStopTaskAsync(taskId);
        }
        else
        {
            var worker = await GetWorker(data, workerHost);

            if (worker != null)
            {
                await AddStopTaskByManualAsync(taskId);

                var @event = new StopTaskIntegrationEvent()
                {
                    TaskId = taskId,
                    ServiceId = worker.ServiceId,
                    Topic = nameof(StopTaskIntegrationEvent) + worker.ServiceId
                };

                await EventBus.PublishAsync(@event);
                await EventBus.CommitAsync();
            }
        }
    }

    public async Task StartAssignAsync()
    {
        var data = ServiceProvider.GetRequiredService<SchedulerServerManagerData>();
        var result = await DequeueTaskAsync();

        if (result.IsNull)
        {
            _schedulerLogger.LogInformation($"Task Dequeue is null", WriterTypes.Server, Guid.Empty, Guid.Empty);
            await Task.Delay(1000);
            return;
        }

        SchedulerTask? task = null;

        try
        {
            var taskId = new Guid(result.ToString());
            task = await _repository.AsQueryable().Include(x => x.Job).FirstOrDefaultAsync(p => p.Id == taskId);
            if (task is null)
            {
                _schedulerLogger.LogInformation($"Task is not found", WriterTypes.Server, Guid.Empty, Guid.Empty);
                await Task.Delay(100);
                return;
            }

            _schedulerLogger.LogInformation($"Task Dequeue", WriterTypes.Server, task.Id, task.JobId);
            if (await ExistsStopTaskAsync(task.Id))
            {
                _schedulerLogger.LogInformation($"Task Stop", WriterTypes.Server, task.Id, task.JobId);
                await RemoveStopTaskAsync(task.Id);
                await Task.Delay(100);
                return;
            }

            WorkerModel? workerModel;

            if (task.Job.RoutingStrategy == RoutingStrategyTypes.Specified)
            {
                workerModel = await GetWorker(data, task.Job.SpecifiedWorkerHost);
            }
            else
            {
                workerModel = await GetWorker(data, task.Job.RoutingStrategy);
            }


            if (workerModel == null || !data.ServiceList.Any(x => x.Status == ServiceStatus.Normal))
            {
                if (task.Job.ScheduleExpiredStrategy == ScheduleExpiredStrategyTypes.Ignore)
                {
                    await EventBus.PublishAsync(new NotifyTaskRunResultDomainEvent(new NotifySchedulerTaskRunResultRequest()
                    {
                        TaskId = task.Id,
                        Status = TaskRunStatus.Ignore,
                        Message = "Worker not available, ignoring task"
                    }));
                    await Task.Delay(100);
                    return;
                }
            }

            if (workerModel == null)
            {
                var notifyEvent = new NotifyTaskRunResultDomainEvent(new NotifySchedulerTaskRunResultRequest()
                {
                    TaskId = task.Id,
                    Status = TaskRunStatus.Failure,
                    Message = "cannot find worker"
                });
                await EventBus.PublishAsync(notifyEvent);

                _schedulerLogger.LogInformation($"Cannot find worker model", WriterTypes.Server, task.Id, task.JobId);
                await Task.Delay(100);
                return;
            }

            await CheckHeartbeat(workerModel!);

            if (workerModel.Status != ServiceStatus.Normal)
            {
                _schedulerLogger.LogInformation($"WorkerModel Status is not Normal, wait to retry", WriterTypes.Server, task.Id, task.JobId);
                await EnqueueTaskAsync(task.Id);
                await Task.Delay(100);
                return;
            }

            task.TaskStart();
            task.SetWorkerHost(workerModel.GetServiceUrl());
            await _repository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            _schedulerLogger.LogInformation($"Sending task to worker, workerHost: {workerModel.GetServiceUrl()}", WriterTypes.Server, task.Id, task.JobId);

            var taskDto = _mapper.Map<SchedulerTaskDto>(task);

            if (taskDto.Job.JobType == JobTypes.JobApp && taskDto.Job.JobAppConfig != null)
            {
                await FillJobAppConfigAsync(taskDto);
            }

            await StartTask(taskDto, workerModel);
            await _signalRUtils.SendNoticationByGroup(ConstStrings.GLOBAL_GROUP, SignalRMethodConsts.GET_NOTIFICATION, taskDto);
        }
        catch (Exception ex)
        {
            if (task != null)
            {
                _schedulerLogger.LogError(ex, $"Task Assign Error", WriterTypes.Server, task.Id, task.JobId);
                await EnqueueTaskAsync(task.Id);
            }
            else
            {
                _logger.LogError(ex, "StartAssignAsync Error");
            }
        }
    }

    private async Task FillJobAppConfigAsync(SchedulerTaskDto taskDto)
    {
        var resourceList = await _resourceRepository.GetListAsync(r => r.JobAppIdentity == taskDto.Job.JobAppConfig.JobAppIdentity, nameof(SchedulerResource.CreationTime));

        var resource = !string.IsNullOrEmpty(taskDto.Job.JobAppConfig.Version) ? resourceList.FirstOrDefault(r => r.Version == taskDto.Job.JobAppConfig.Version) : resourceList.FirstOrDefault();

        if (resource != null)
        {
            taskDto.Job.JobAppConfig.SchedulerResourceDto = _mapper.Map<SchedulerResourceDto>(resource);
        }
    }

    private async Task StartTask(SchedulerTaskDto taskDto, WorkerModel worker)
    {
        var @event = new StartTaskIntegrationEvent()
        {
            TaskId = taskDto.Id,
            Job = taskDto.Job,
            ServiceId = worker.ServiceId,
            ExcuteTime = taskDto.SchedulerTime
        };
        @event.Topic = nameof(StartTaskIntegrationEvent) + worker.ServiceId;

        await EventBus.PublishAsync(@event);
        await EventBus.CommitAsync();
    }

    private async Task EnqueueTaskAsync(Guid taskId)
    {
        var taskQueueKey = ConstStrings.TaskQueueKey(_multiEnvironmentContext.CurrentEnvironment);
        await _redis.ListRightPushAsync(taskQueueKey, taskId.ToString());

        var taskSetKey = ConstStrings.TaskSetKey(_multiEnvironmentContext.CurrentEnvironment);
        await _redis.SetAddAsync(taskSetKey, taskId.ToString());
    }

    private async Task<RedisValue> DequeueTaskAsync()
    {
        var taskQueueKey = ConstStrings.TaskQueueKey(_multiEnvironmentContext.CurrentEnvironment);
        var result = await _redis.ListLeftPopAsync(taskQueueKey);

        var taskSetKey = ConstStrings.TaskSetKey(_multiEnvironmentContext.CurrentEnvironment);
        await _redis.SetRemoveAsync(taskSetKey, result.ToString());

        return result;
    }

    private async Task<bool> ExistsTaskAsync(Guid taskId)
    {
        var taskSetKey = ConstStrings.TaskSetKey(_multiEnvironmentContext.CurrentEnvironment);
        return await _redis.SetContainsAsync(taskSetKey, taskId.ToString());
    }

    private async Task AddStopTaskAsync(Guid taskId)
    {
        await _redis.SetAddAsync(ConstStrings.STOP_TASK_KEY, taskId.ToString());
    }

    private async Task<bool> ExistsStopTaskAsync(Guid taskId)
    {
        return await _redis.SetContainsAsync(ConstStrings.STOP_TASK_KEY, taskId.ToString());
    }

    private async Task RemoveStopTaskAsync(Guid taskId)
    {
        await _redis.SetRemoveAsync(ConstStrings.STOP_TASK_KEY, taskId.ToString());
    }

    private async Task AddStopTaskByManualAsync(Guid taskId)
    {
        await _redis.SetAddAsync(ConstStrings.STOP_BY_MANUAL_KEY, taskId.ToString());
    }
}
