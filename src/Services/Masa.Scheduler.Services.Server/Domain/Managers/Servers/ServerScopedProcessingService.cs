﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class ServerScopedProcessingService : IScopedProcessingService
{
    private readonly ILogger<ServerScopedProcessingService> _logger;
    private readonly SchedulerDbContext _dbContext;
    private readonly IRepository<SchedulerJob> _jobRepository;
    private readonly IEventBus _eventBus;
    private readonly QuartzUtils _quartzUtils;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;
    private readonly IServiceScopeFactory _scopeFactory;

    public ServerScopedProcessingService(
        ILogger<ServerScopedProcessingService> logger,
        SchedulerDbContext dbContext,
        IRepository<SchedulerJob> jobRepository,
        IEventBus eventBus,
        QuartzUtils quartzUtils,
        IMultiEnvironmentContext multiEnvironmentContext,
        IServiceScopeFactory scopeFactory)  
    {
        _logger = logger;
        _dbContext = dbContext;
        _jobRepository = jobRepository;
        _eventBus = eventBus;
        _quartzUtils = quartzUtils;
        _multiEnvironmentContext = multiEnvironmentContext;
        _scopeFactory = scopeFactory;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        await OnManagerStartAsync();
    }

    private async Task OnManagerStartAsync()
    {
        await StartAssignAsync();

        await LoadRunningAndRetryTaskAsync();

        var cronJobList = await _jobRepository.GetListAsync(job => job.ScheduleType == ScheduleTypes.Cron && !string.IsNullOrEmpty(job.CronExpression) && job.Enabled);

        await CheckSchedulerExpiredJobAsync(cronJobList.Where(p => p.ScheduleExpiredStrategy != ScheduleExpiredStrategyTypes.Ignore));

        await RegisterCronJobAsync(cronJobList);
    }

    private async Task LoadRunningAndRetryTaskAsync()
    {
        const int pageSize = 1000;
        int pageNumber = 1;

        do
        {
            var pageTasks = await LoadTasksAsync(pageSize, pageNumber);

            if (pageTasks.Count == 0)
            {
                break;
            }

            await LoadRunningTaskAsync(pageTasks);

            await LoadRetryTaskAsync(pageTasks);

            pageNumber++;
        } while (true);
    }

    private async Task<List<SchedulerTask>> LoadTasksAsync(int pageSize, int pageNumber)
    {
        return await _dbContext.Tasks
            .Include(t => t.Job)
            .Where(t => (t.TaskStatus == TaskRunStatus.Running || t.TaskStatus == TaskRunStatus.WaitToRetry) && t.Job.Enabled)
            .OrderBy(t => t.CreationTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
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

            await _eventBus.PublishAsync(startTaskDomainEvent);
        }
    }

    private async Task LoadRetryTaskAsync(List<SchedulerTask> allTask)
    {
        var retryTaskList = allTask.FindAll(t => t.TaskStatus == TaskRunStatus.WaitToRetry);

        foreach (var retryTask in retryTaskList)
        {
            await _quartzUtils.AddDelayTask<StartSchedulerTaskQuartzJob>(_multiEnvironmentContext.CurrentEnvironment, retryTask.Id, retryTask.Job.Id, TimeSpan.FromSeconds(retryTask.Job.FailedRetryInterval));
        }
    }

    private async Task CheckSchedulerExpiredJobAsync(IEnumerable<SchedulerJob> cronJobList)
    {
        foreach (var cronJob in cronJobList)
        {
            if (cronJob.ScheduleExpiredStrategy == ScheduleExpiredStrategyTypes.Ignore)
            {
                continue;
            }

            var request = new StartSchedulerJobRequest()
            {
                JobId = cronJob.Id,
                OperatorId = Guid.Empty
            };

            var calcStartTime = cronJob.UpdateExpiredStrategyTime;

            var lastTask = await _dbContext.Tasks.Where(x => x.JobId == cronJob.Id).OrderByDescending(t => t.CreationTime).AsNoTracking().FirstOrDefaultAsync();

            if (lastTask != null && calcStartTime < lastTask.SchedulerTime)
            {
                calcStartTime = lastTask.SchedulerTime;
            }

            _logger.LogInformation($"Test ScheduleExpiredStrategy, currentTime: {DateTimeOffset.UtcNow}, calcStartTime: {calcStartTime}, JobId: {cronJob.Id}");

            var excuteTimeList = await _quartzUtils.GetCronExcuteTimeByTimeRange(cronJob.CronExpression, calcStartTime, DateTimeOffset.UtcNow);

            _logger.LogInformation($"ExcuteTimeList, excuteTimeList: {JsonSerializer.Serialize(excuteTimeList)}, JobId: {cronJob.Id}");

            switch (cronJob.ScheduleExpiredStrategy)
            {
                case ScheduleExpiredStrategyTypes.ExecuteImmediately:
                    if (excuteTimeList.Any())
                    {
                        request.ExcuteTime = DateTimeOffset.UtcNow;
                        await _eventBus.PublishAsync(new StartJobDomainEvent(request));
                    }
                    break;
                case ScheduleExpiredStrategyTypes.AutoCompensation:
                    foreach (var excuteTime in excuteTimeList)
                    {
                        request.ExcuteTime = excuteTime;
                        await _eventBus.PublishAsync(new StartJobDomainEvent(request));
                    }
                    break;
                default:
                    continue;
            }
        }
    }

    private async Task RegisterCronJobAsync(IEnumerable<SchedulerJob> cronJobList)
    {
        foreach (var cronJob in cronJobList)
        {
            try
            {
                await _quartzUtils.RegisterCronJob<StartSchedulerJobQuartzJob>(_multiEnvironmentContext.CurrentEnvironment, cronJob.Id, cronJob.CronExpression);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RegisterCronJob Error, JobId: {cronJob.Id}, CronExpression: {cronJob.CronExpression}");
            }
        }
    }

    public Task StartAssignAsync()
    {
        Task.Run(async () =>
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            while (true)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var multiEnvironmentSetter = scope.ServiceProvider.GetRequiredService<IMultiEnvironmentSetter>();
                    multiEnvironmentSetter.SetEnvironment(_multiEnvironmentContext.CurrentEnvironment);
                    var schedulerServerManager = scope.ServiceProvider.GetRequiredService<SchedulerServerManager>();

                    await schedulerServerManager.StartAssignAsync();

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background task execution failed, will continue to retry");
                }
                
            }
        });

        return Task.CompletedTask;
    }
}
