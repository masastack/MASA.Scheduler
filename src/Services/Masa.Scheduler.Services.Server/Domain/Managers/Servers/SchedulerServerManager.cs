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
    private readonly IRepository<SchedulerTask> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SignalRUtils _signalRUtils;

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
        IRepository<SchedulerTask> repository,
        IUnitOfWork unitOfWork,
        SignalRUtils signalRUtils)
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
        var taskDto = _mapper.Map<SchedulerTaskDto>(task);

        if (taskDto.Job.JobType == JobTypes.JobApp && taskDto.Job.JobAppConfig != null)
        {
            var resourceList = await _resourceRepository.GetListAsync(r => r.JobAppIdentity == taskDto.Job.JobAppConfig.JobAppIdentity, nameof(SchedulerResource.CreationTime));

            var resource = !string.IsNullOrEmpty(taskDto.Job.JobAppConfig.Version) ? resourceList.FirstOrDefault(r => r.Version == taskDto.Job.JobAppConfig.Version) : resourceList.FirstOrDefault();

            if (resource != null)
            {
                taskDto.Job.JobAppConfig.SchedulerResourceDto = _mapper.Map<SchedulerResourceDto>(resource);
            }
        }

        var data = ServiceProvider.GetRequiredService<SchedulerServerManagerData>();

        _schedulerLogger.LogInformation($"Task Enqueue, ResourceId: {taskDto.Job?.JobAppConfig?.SchedulerResourceDto?.Id}", WriterTypes.Server, taskDto.Id, taskDto.JobId);

        var taskQueue = data.TaskQueue.GetValueOrDefault(_multiEnvironmentContext.CurrentEnvironment);

        if (taskQueue != null)
        {
            taskQueue.Enqueue(taskDto);
        }
    }

    public async Task StopTask(Guid taskId, string workerHost)
    {
        var data = ServiceProvider.GetRequiredService<SchedulerServerManagerData>();

        if (data.TaskQueue.Any(p => p.Value.Any(x => x.Id == taskId)))
        {
            data.StopTask.Add(taskId);
        }
        else
        {
            var worker = await GetWorker(data, workerHost);

            if (worker != null)
            {
                data.StopByManual.Add(taskId);

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

        var taskQueue = data.TaskQueue.GetValueOrDefault(_multiEnvironmentContext.CurrentEnvironment);

        if (taskQueue == null || taskQueue.Count == 0)
        {
            await Task.Delay(100);
            return;
        }

        SchedulerTaskDto? taskDto = null;

        try
        {
            if (taskQueue.TryDequeue(out taskDto))
            {
                _schedulerLogger.LogInformation($"Task Dequeue", WriterTypes.Server, taskDto.Id, taskDto.JobId);

                if (data.StopTask.Any(p => p == taskDto.Id))
                {
                    _schedulerLogger.LogInformation($"Task Stop", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                    data.StopTask.Remove(taskDto.Id);
                    await Task.Delay(100);
                    return;
                }

                WorkerModel? workerModel;

                if (taskDto.Job.RoutingStrategy == RoutingStrategyTypes.Specified)
                {
                    workerModel = await GetWorker(data, taskDto.Job.SpecifiedWorkerHost);
                }
                else
                {
                    workerModel = await GetWorker(data, taskDto.Job.RoutingStrategy);
                }

                var task = await _repository.FindAsync(p => p.Id == taskDto.Id);

                if (task == null)
                {
                    _schedulerLogger.LogInformation($"Task is not found", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                    await Task.Delay(100);
                    return;
                }

                if (workerModel == null || !data.ServiceList.Any(x => x.Status == ServiceStatus.Normal))
                {
                    if (taskDto.Job.ScheduleExpiredStrategy == ScheduleExpiredStrategyTypes.Ignore)
                    {
                        await EventBus.PublishAsync(new NotifyTaskRunResultDomainEvent(new NotifySchedulerTaskRunResultRequest()
                        {
                            TaskId = taskDto.Id,
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
                        TaskId = taskDto.Id,
                        Status = TaskRunStatus.Failure,
                        Message = "cannot find worker"
                    });
                    await EventBus.PublishAsync(notifyEvent);

                    _schedulerLogger.LogInformation($"Cannot find worker model", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                    await Task.Delay(100);
                    return;
                }

                await CheckHeartbeat(workerModel!);

                if (workerModel.Status != ServiceStatus.Normal)
                {
                    _schedulerLogger.LogInformation($"WorkerModel Status is not Normal, wait to retry", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                    taskQueue.Enqueue(taskDto);
                    await Task.Delay(100);
                    return;
                }

                task.TaskStart();
                task.SetWorkerHost(workerModel.GetServiceUrl());
                await _repository.UpdateAsync(task);
                await _unitOfWork.SaveChangesAsync();

                _schedulerLogger.LogInformation($"Sending task to worker, workerHost: {workerModel.GetServiceUrl()}", WriterTypes.Server, taskDto.Id, taskDto.JobId);

                await StartTask(taskDto, workerModel);
                await _signalRUtils.SendNoticationByGroup(ConstStrings.GLOBAL_GROUP, SignalRMethodConsts.GET_NOTIFICATION, _mapper.Map<SchedulerTaskDto>(task));
            }
        }
        catch (Exception ex)
        {
            if (taskDto != null)
            {
                _schedulerLogger.LogError(ex, $"Task Assign Error", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                taskQueue.Enqueue(taskDto);
            }
            else
            {
                _logger.LogError(ex, "StartAssignAsync Error");
            }
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
}
