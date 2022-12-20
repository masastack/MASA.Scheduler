// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class SchedulerServerManager : BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>
{
    private readonly ILogger<SchedulerServerManager> _logger;
    private readonly IMapper _mapper;
    private readonly IRepository<SchedulerTask> _repository;
    private readonly IRepository<SchedulerResource> _resourceRepository;
    private readonly IRepository<SchedulerJob> _jobRepository;
    private readonly SchedulerDbContext _dbContext;
    private readonly QuartzUtils _quartzUtils;
    private readonly IDomainEventBus _domainEventBus;
    private readonly SchedulerLogger _schedulerLogger;
    private readonly SignalRUtils _signalRUtils;
    private static string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;

    public SchedulerServerManager(
        IDistributedCacheClientFactory cacheClientFactory,
        IDistributedCacheClient redisCacheClient,
        IServiceProvider serviceProvider,
        IIntegrationEventBus eventBus,
        ILogger<SchedulerServerManager> logger,
        IHttpClientFactory httpClientFactory,
        SchedulerServerManagerData data,
        IMapper mapper,
        IRepository<SchedulerTask> repository,
        IHostApplicationLifetime hostApplicationLifetime,
        IRepository<SchedulerResource> resourceRepository,
        IRepository<SchedulerJob> jobRepository,
        QuartzUtils quartzUtils,
        SchedulerDbContext dbContext,
        IDomainEventBus domainEventBus,
        SchedulerLogger schedulerLogger,
        SignalRUtils signalRUtils)
        : base(cacheClientFactory,
               redisCacheClient,
               serviceProvider,
               eventBus,
               httpClientFactory,
               data,
               hostApplicationLifetime)
    {
        _logger = logger;
        _mapper = mapper;
        _repository = repository;
        _resourceRepository = resourceRepository;
        _jobRepository = jobRepository;
        _quartzUtils = quartzUtils;
        _dbContext = dbContext;
        _domainEventBus = domainEventBus;
        _schedulerLogger = schedulerLogger;
        _signalRUtils = signalRUtils;
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_SERVER_MANAGER_API}/heartbeat";

    protected override string OnlineApi { get; set; } = $"{ConstStrings.SCHEDULER_SERVER_MANAGER_API}/online";

    protected override string OnlineTopic { get; set; } = $"{nameof(SchedulerServerOnlineIntegrationEvent)}-{envName}";

    protected override string MoniterTopic { get; set; } = $"{nameof(SchedulerWorkerOnlineIntegrationEvent)}-{envName}";

    protected override ILogger<BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>> Logger => _logger;

    public override async Task OnManagerStartAsync()
    {
        await base.OnManagerStartAsync();

        await StartAssignAsync();

        await _quartzUtils.StartQuartzScheduler();

        var allTask = await _dbContext.Tasks.Include(t => t.Job).Where(t => (t.TaskStatus == TaskRunStatus.Running || t.TaskStatus == TaskRunStatus.WaitToRetry) && t.Job.Enabled).AsNoTracking().ToListAsync();

        await LoadRunningTaskAsync(allTask);

        await LoadRetryTaskAsync(allTask);

        var cronJobList = await _jobRepository.GetListAsync(job => job.ScheduleType == ScheduleTypes.Cron && !string.IsNullOrEmpty(job.CronExpression) && job.Enabled);

        await CheckSchedulerExpiredJobAsync(cronJobList.Where(p => p.ScheduleExpiredStrategy != ScheduleExpiredStrategyTypes.Ignore));

        await RegisterCronJobAsync(cronJobList);
    }

    private async Task RegisterCronJobAsync(IEnumerable<SchedulerJob> cronJobList)
    {
        foreach (var cronJob in cronJobList)
        {
            try
            {
                await _quartzUtils.RegisterCronJob<StartSchedulerJobQuartzJob>(cronJob.Id, cronJob.CronExpression);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, $"RegisterCronJob Error, JobId: {cronJob.Id}, CronExpression: {cronJob.CronExpression}");
            }
        }
    }

    private async Task LoadRunningTaskAsync(List<SchedulerTask> allTask)
    {
        var runningTaskList = allTask.FindAll(t => t.TaskStatus == TaskRunStatus.Running);

        foreach (var runningTask in runningTaskList)
        {
            var startTaskDomainEvent = new StartTaskDomainEvent(new StartSchedulerTaskRequest()
            {
                TaskId = runningTask.Id,
                ExcuteTime = runningTask.SchedulerTime,
                OperatorId = runningTask.OperatorId,
                IsManual = true
            }, runningTask);

            await _domainEventBus.PublishAsync(startTaskDomainEvent);
        }
    }

    private async Task CheckSchedulerExpiredJobAsync(IEnumerable<SchedulerJob> cronJobList)
    {
        foreach (var cronJob in cronJobList)
        {
            if(cronJob.ScheduleExpiredStrategy == ScheduleExpiredStrategyTypes.Ignore)
            {
                continue;
            }

            var request = new StartSchedulerJobRequest()
            {
                JobId = cronJob.Id,
                OperatorId = Guid.Empty
            };

            var calcStartTime = cronJob.UpdateExpiredStrategyTime;

            var lastTask = await _dbContext.Tasks.OrderByDescending(t => t.SchedulerTime).AsNoTracking().FirstOrDefaultAsync();

            if(lastTask != null && calcStartTime < lastTask.SchedulerTime)
            {
                calcStartTime = lastTask.SchedulerTime;
            }

            Logger.LogInformation($"Test ScheduleExpiredStrategy, currentTime: {DateTimeOffset.Now}, calcStartTime: {calcStartTime}, JobId: {cronJob.Id}");

            var excuteTimeList = await _quartzUtils.GetCronExcuteTimeByTimeRange(cronJob.CronExpression, calcStartTime, DateTimeOffset.Now);

            Logger.LogInformation($"ExcuteTimeList, excuteTimeList: {JsonSerializer.Serialize(excuteTimeList)}, JobId: {cronJob.Id}");

            switch (cronJob.ScheduleExpiredStrategy)
            {
                case ScheduleExpiredStrategyTypes.ExecuteImmediately:
                    if (excuteTimeList.Any())
                    {
                        request.ExcuteTime = DateTimeOffset.Now;
                        await _domainEventBus.PublishAsync(new StartJobDomainEvent(request));
                    }
                    break;
                case ScheduleExpiredStrategyTypes.AutoCompensation:
                    foreach (var excuteTime in excuteTimeList)
                    {
                        request.ExcuteTime = excuteTime;
                        await _domainEventBus.PublishAsync(new StartJobDomainEvent(request));
                    }
                    break;
                default:
                    continue;
            }
        }
    }

    private async Task LoadRetryTaskAsync(List<SchedulerTask> allTask)
    {
        var retryTaskList = allTask.FindAll(t => t.TaskStatus == TaskRunStatus.WaitToRetry);

        foreach (var retryTask in retryTaskList)
        {
            await _quartzUtils.AddDelayTask<StartSchedulerTaskQuartzJob>(retryTask.Id, retryTask.Job.Id, TimeSpan.FromSeconds(retryTask.Job.FailedRetryInterval));
        }
    }

    public async Task<WorkerModel?> GetWorker(SchedulerServerManagerData data,RoutingStrategyTypes routingType)
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
                Console.WriteLine($"CurrentRunCount: {currentRunCount}, currentUesIndex: {currentUesIndex}, serviceCount: {serviceCount}");
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
       
        if(uri.Scheme == Uri.UriSchemeHttp)
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

        if(taskDto.Job.JobType == JobTypes.JobApp && taskDto.Job.JobAppConfig != null)
        {
            var resourceList = await _resourceRepository.GetListAsync(r => r.JobAppIdentity == taskDto.Job.JobAppConfig.JobAppIdentity, nameof(SchedulerResource.CreationTime));

            var resource = !string.IsNullOrEmpty(taskDto.Job.JobAppConfig.Version) ? resourceList.FirstOrDefault(r => r.Version == taskDto.Job.JobAppConfig.Version) : resourceList.FirstOrDefault();

            if(resource != null)
            {
                taskDto.Job.JobAppConfig.SchedulerResourceDto = _mapper.Map<SchedulerResourceDto>(resource);
            }
        }

        await using var scope = ServiceProvider.CreateAsyncScope();

        var data = scope.ServiceProvider.GetRequiredService<SchedulerServerManagerData>();

        _schedulerLogger.LogInformation($"Task Enqueue, ResourceId: {taskDto.Job?.JobAppConfig?.SchedulerResourceDto?.Id}", WriterTypes.Server, taskDto.Id, taskDto.JobId);

        data.TaskQueue.Enqueue(taskDto);
    }

    public async Task StartTask(IServiceProvider provider, SchedulerTaskDto taskDto, WorkerModel worker)
    {
        var @event = new StartTaskIntegrationEvent()
        {
            TaskId = taskDto.Id,
            Job = taskDto.Job,
            ServiceId = worker.ServiceId,
            ExcuteTime = taskDto.SchedulerTime
        };
        @event.Topic = nameof(StartTaskIntegrationEvent) + worker.ServiceId;

        var eventBus = provider.GetRequiredService<IIntegrationEventBus>();
        
        await eventBus.PublishAsync(@event);
        await eventBus.CommitAsync();
    }

    public async Task StopTask(Guid taskId, string workerHost)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var data = scope.ServiceProvider.GetRequiredService<SchedulerServerManagerData>();

        if(data.TaskQueue.Any(p => p.Id == taskId))
        {
            data.StopTask.Add(taskId);
        }
        else
        {
            var worker = await GetWorker(data, workerHost);

            if(worker != null)
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
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            while (true)
            {
                await using var scope = ServiceProvider.CreateAsyncScope();

                var provider = scope.ServiceProvider;

                var data = provider.GetRequiredService<SchedulerServerManagerData>();

                if (data.TaskQueue.Count == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                if(!data.ServiceList.Any())
                {
                    await Task.Delay(100);
                    continue;
                }

                SchedulerTaskDto? taskDto = null;

                try
                {
                    if (data.TaskQueue.TryDequeue(out taskDto))
                    {
                        _schedulerLogger.LogInformation($"Task Dequeue", WriterTypes.Server, taskDto.Id, taskDto.JobId);

                        if (data.StopTask.Any(p => p == taskDto.Id))
                        {
                            _schedulerLogger.LogInformation($"Task Stop", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                            data.StopTask.Remove(taskDto.Id);
                            continue;
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

                        var repository = provider.GetRequiredService<IRepository<SchedulerTask>>();

                        var task = await repository.FindAsync(p => p.Id == taskDto.Id);

                        if (task == null)
                        {
                            _schedulerLogger.LogInformation($"Task is not found", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                            continue;
                        }

                        if (workerModel == null)
                        {
                            var notifyEvent = new NotifyTaskRunResultDomainEvent(new NotifySchedulerTaskRunResultRequest()
                            {
                                TaskId = taskDto.Id,
                                Status = TaskRunStatus.Failure,
                                Message = "cannot find worker"
                            });

                            await _domainEventBus.PublishAsync(notifyEvent);

                            _schedulerLogger.LogInformation($"Cannot find worker model", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                            continue;
                        }

                        await CheckHeartbeat(workerModel!);

                        if(workerModel.Status != ServiceStatus.Normal)
                        {
                            _schedulerLogger.LogInformation($"WorkerModel Status is not Normal, wait to retry", WriterTypes.Server, taskDto.Id, taskDto.JobId);                            
                            data.TaskQueue.Enqueue(taskDto);
                            await Task.Delay(100);
                            continue;
                        }

                        task.TaskStart();
                        task.SetWorkerHost(workerModel.GetServiceUrl());

                        await repository.UpdateAsync(task);

                        await repository.UnitOfWork.SaveChangesAsync();

                        _schedulerLogger.LogInformation($"Sending task to worker, workerHost: {workerModel.GetServiceUrl()}", WriterTypes.Server, taskDto.Id, taskDto.JobId);

                        await StartTask(provider, taskDto, workerModel);

                        await _signalRUtils.SendNoticationByGroup(ConstStrings.GLOBAL_GROUP, SignalRMethodConsts.GET_NOTIFICATION, _mapper.Map<SchedulerTaskDto>(task));
                    }
                }
                catch(Exception ex)
                {
                    if(taskDto != null)
                    {
                        _schedulerLogger.LogError(ex, $"Task Assign Error", WriterTypes.Server, taskDto.Id, taskDto.JobId);
                        data.TaskQueue.Enqueue(taskDto);
                    }
                    else
                    {
                        _logger.LogError(ex, "StartAssignAsync Error");
                    }
                }
            }
        });

        return Task.CompletedTask;
    }
}
