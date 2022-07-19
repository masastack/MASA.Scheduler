// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class SchedulerServerManager : BaseSchedulerManager<WorkerModel, SchedulerServerOnlineIntegrationEvent, SchedulerWorkerOnlineIntegrationEvent>
{
    private readonly ILogger<SchedulerServerManager> _logger;
    private readonly IMapper _mapper;
    private readonly SchedulerServerManagerData _data;
    private readonly IRepository<SchedulerTask> _repository;
    private readonly IRepository<SchedulerResource> _resourceRepository;
    private readonly IRepository<SchedulerJob> _jobRepository;
    private readonly SchedulerDbContext _dbContext;
    private readonly QuartzUtils _quartzUtils;
    private readonly IDomainEventBus _domainEventBus;

    public SchedulerServerManager(
        IDistributedCacheClientFactory cacheClientFactory,
        IDistributedCacheClient redisCacheClient,
        IServiceProvider serviceProvider,
        IIntegrationEventBus eventBus,
        ILogger<SchedulerServerManager> logger,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        SchedulerServerManagerData data,
        IRepository<SchedulerTask> repository,
        IHostApplicationLifetime hostApplicationLifetime,
        IRepository<SchedulerResource> resourceRepository,
        IRepository<SchedulerJob> jobRepository,
        QuartzUtils quartzUtils,
        SchedulerDbContext dbContext,
        IDomainEventBus domainEventBus)
        : base(cacheClientFactory, redisCacheClient, serviceProvider, eventBus, httpClientFactory, data, hostApplicationLifetime)
    {
        _logger = logger;
        _mapper = mapper;
        _data = data;
        _repository = repository;
        _resourceRepository = resourceRepository;
        _jobRepository = jobRepository;
        _quartzUtils = quartzUtils;
        _dbContext = dbContext;
        _domainEventBus = domainEventBus;
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_SERVER_MANAGER_API}/heartbeat";

    protected override string OnlineApi { get; set; } = $"{ConstStrings.SCHEDULER_SERVER_MANAGER_API}/online";

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

        await RegisterCronJobAsync(cronJobList);

        await CheckSchedulerExpiredJobAsync(cronJobList);
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
            await TaskEnqueue(runningTask);
        }
    }

    private async Task CheckSchedulerExpiredJobAsync(IEnumerable<SchedulerJob> cronJobList)
    {
        foreach (var cronJob in cronJobList)
        {
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

            var excuteTimeList = await _quartzUtils.GetCronExcuteTimeByTimeRange(cronJob.CronExpression, calcStartTime, DateTimeOffset.Now);

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

    public async Task<WorkerModel> GetWorker(SchedulerServerManagerData data,RoutingStrategyTypes routingType)
    {
        if (!data.ServiceList.FindAll(w => w.Status == ServiceStatus.Normal).Any())
        {
            throw new UserFriendlyException("WorkerList is empty");
        }

        WorkerModel? worker = null;

        switch (routingType)
        {
            case RoutingStrategyTypes.RoundRobin:
                var currentRunCount = await RedisCacheClient.HashIncrementAsync(CacheKeys.CURRENT_RUN_COUNT);
                var currentUesIndex = Convert.ToInt32((currentRunCount - 1) % data.ServiceList.FindAll(w => w.Status == ServiceStatus.Normal).Count);
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
       
        if(uri.Scheme == Uri.UriSchemeHttp)
        {
            workerModel = ServiceList.FirstOrDefault(w => w.HttpServiceUrl == workerHost && w.Status == ServiceStatus.Normal);
        }
        else
        {
            workerModel = ServiceList.FirstOrDefault(w => w.HttpsServiceUrl == workerHost && w.Status == ServiceStatus.Normal);
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

        _logger.LogInformation($"SchedulerServerManager: TaskEnqueue, JobId: {task.JobId}, TaskId: {task.Id}");

        _data.TaskQueue.Enqueue(taskDto);
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

        var eventBus = provider.GetRequiredService<IIntegrationEventBus>();
        await eventBus.PublishAsync(@event);
        await eventBus.CommitAsync();
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
                _data.StopByManual.Add(taskId);

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
                        _logger.LogInformation($"SchedulerServerManager: TaskDequeue, JobId: {taskDto.JobId}, TaskId: {taskDto.Id}");

                        if (data.StopTask.Any(p => p == taskDto.Id))
                        {
                            _logger.LogInformation($"SchedulerServerManager: TaskStop, JobId: {taskDto.JobId}, TaskId: {taskDto.Id}");
                            data.StopTask.Remove(taskDto.Id);
                            continue;
                        }

                        WorkerModel? workerModel;

                        if (taskDto.Job.RoutingStrategy == RoutingStrategyTypes.Specified)
                        {
                            workerModel = await GetWorker(taskDto.Job.SpecifiedWorkerHost);

                            if(workerModel == null)
                            {
                                _logger.LogError($"SchedulerServerManager: Cannot find worker model, workerHost: {taskDto.Job.SpecifiedWorkerHost}, JobId: {taskDto.JobId}, TaskId: {taskDto.Id}");
                                continue;
                            }
                        }
                        else
                        {
                            workerModel = await GetWorker(data, taskDto.Job.RoutingStrategy);
                        }

                        await CheckHeartbeat(workerModel!);

                        if(workerModel.Status != ServiceStatus.Normal)
                        {
                            _logger.LogInformation($"SchedulerServerManager: WorkerModel Status is not Normal, JobId: {taskDto.JobId}, TaskId: {taskDto.Id}");
                            data.TaskQueue.Enqueue(taskDto);
                            await Task.Delay(1000);
                        }

                        var repository = provider.GetRequiredService<IRepository<SchedulerTask>>();

                        var task = await repository.FindAsync(p=> p.Id == taskDto.Id);

                        if(task == null)
                        {
                            _logger.LogError($"SchedulerServerManager:Task is not found, JobId: {taskDto.JobId}, TaskId: {taskDto.Id}");
                            throw new UserFriendlyException($"Task is not found, taskId: {taskDto.Id}");
                        }

                        task.TaskStart();
                        task.SetWorkerHost(workerModel.GetServiceUrl());

                        await repository.UpdateAsync(task);

                        await repository.UnitOfWork.SaveChangesAsync();

                        await StartTask(provider, taskDto, workerModel);
                    }
                }
                catch(Exception ex)
                {
                    if(taskDto != null)
                    {
                        _logger.LogError(ex, $"SchedulerServerManager:Task Assign Error, Exception Message: {ex.Message}, JobId: {taskDto.JobId}, TaskId: {taskDto.Id}");
                        _data.TaskQueue.Enqueue(taskDto);
                    }
                    else
                    {
                        Logger.LogError(ex, "StartAssignAsync");
                    }
                }
            }
        });

        return Task.CompletedTask;
    }
}
