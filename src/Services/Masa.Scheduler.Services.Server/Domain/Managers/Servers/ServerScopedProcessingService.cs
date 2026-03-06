// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public class ServerScopedProcessingService : IScopedProcessingService
{
    private readonly ILogger<ServerScopedProcessingService> _logger;
    private readonly SchedulerDbContext _dbContext;
    private readonly IRepository<SchedulerJob> _jobRepository;
    private readonly IEventBus _eventBus;
    private readonly ISchedulerBackend _schedulerBackend;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<SchedulerBackendOptions> _schedulerOptions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;

    public ServerScopedProcessingService(
        ILogger<ServerScopedProcessingService> logger,
        SchedulerDbContext dbContext,
        IRepository<SchedulerJob> jobRepository,
        IEventBus eventBus,
        ISchedulerBackend schedulerBackend,
        IMultiEnvironmentContext multiEnvironmentContext,
        IServiceScopeFactory scopeFactory,
        IOptions<SchedulerBackendOptions> schedulerOptions,
        IHttpClientFactory httpClientFactory,
        IServiceProvider serviceProvider)  
    {
        _logger = logger;
        _dbContext = dbContext;
        _jobRepository = jobRepository;
        _eventBus = eventBus;
        _schedulerBackend = schedulerBackend;
        _multiEnvironmentContext = multiEnvironmentContext;
        _scopeFactory = scopeFactory;
        _schedulerOptions = schedulerOptions;
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        await OnManagerStartAsync();
    }

    private async Task OnManagerStartAsync()
    {
        await CleanupOtherBackendAsync();

        await StartAssignAsync();

        //await LoadRunningAndRetryTaskAsync();

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
            await _schedulerBackend.AddDelayTask(_multiEnvironmentContext.CurrentEnvironment, retryTask.Id, retryTask.Job.Id, TimeSpan.FromSeconds(retryTask.Job.FailedRetryInterval));
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

            var excuteTimeList = await _schedulerBackend.GetCronExecuteTimeByTimeRange(cronJob.CronExpression, calcStartTime, DateTimeOffset.UtcNow);

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
                await _schedulerBackend.RegisterCronJob(_multiEnvironmentContext.CurrentEnvironment, cronJob.Id, cronJob.CronExpression);
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

    private async Task CleanupOtherBackendAsync()
    {
        var options = _schedulerOptions.Value;
        if (!options.CleanupOtherBackendOnStart)
        {
            return;
        }

        var useDapr = string.Equals(options.Backend, SchedulerBackendType.DaprJobs, StringComparison.OrdinalIgnoreCase);
        if (useDapr)
        {
            await CleanupQuartzJobsAsync();
        }
        else
        {
            await CleanupDaprJobsAsync();
        }
    }

    private async Task CleanupQuartzJobsAsync()
    {
        try
        {
            var quartzUtils = _serviceProvider.GetService<Infrastructure.Quartz.QuartzUtils>();
            if (quartzUtils == null)
            {
                _logger.LogWarning("CleanupQuartzJobs skipped because QuartzUtils is not registered.");
                return;
            }

            await quartzUtils.ClearAllJobsAsync();
            _logger.LogInformation("Quartz jobs cleared because backend switched to DaprJobs.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CleanupQuartzJobs failed; will continue startup.");
        }
    }

    private async Task CleanupDaprJobsAsync()
    {
        try
        {
            if (!await WaitForDaprSidecarReadyAsync())
            {
                _logger.LogWarning("CleanupDaprJobs skipped because Dapr sidecar is not ready.");
                return;
            }

            var environment = _multiEnvironmentContext.CurrentEnvironment;
            var cronJobs = await _jobRepository.GetListAsync(job =>
                job.ScheduleType == ScheduleTypes.Cron
                && !string.IsNullOrEmpty(job.CronExpression));

            foreach (var cronJob in cronJobs)
            {
                var name = DaprJobsNameHelper.BuildCronName(environment, cronJob.Id);
                await TryDeleteDaprJobAsync(name);
            }

            var retryTasks = await _dbContext.Tasks
                .Where(task => task.TaskStatus == TaskRunStatus.WaitToRetry && task.Job.Enabled)
                .Select(task => new { task.Id, task.JobId })
                .ToListAsync();

            foreach (var retryTask in retryTasks)
            {
                var name = DaprJobsNameHelper.BuildRetryName(environment, retryTask.JobId, retryTask.Id);
                await TryDeleteDaprJobAsync(name);
            }

            _logger.LogInformation("Dapr Jobs cleared because backend switched to Quartz.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CleanupDaprJobs failed; will continue startup.");
        }
    }

    private async Task TryDeleteDaprJobAsync(string name)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var endpoint = BuildDaprHttpEndpoint($"/v1.0-alpha1/jobs/{Uri.EscapeDataString(name)}");
            var response = await client.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                if (IsCronClosed(body))
                {
                    _logger.LogWarning("TryDeleteDaprJobAsync ignored. Name: {Name}. Status: {Status}. Body: {Body}", name, response.StatusCode, body);
                    return;
                }
                _logger.LogError("TryDeleteDaprJobAsync failed. Name: {Name}. Status: {Status}. Body: {Body}", name, response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TryDeleteDaprJobAsync failed. Name: {Name}", name);
            throw;
        }
    }

    private static bool IsDaprNotFound(Exception exception)
    {
        return exception.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("404", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildDaprHttpEndpoint(string path)
    {
        var endpoint = Environment.GetEnvironmentVariable("DAPR_HTTP_ENDPOINT");
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            var port = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";
            endpoint = $"http://127.0.0.1:{port}";
        }

        return $"{endpoint.TrimEnd('/')}{path}";
    }

    private async Task<bool> WaitForDaprSidecarReadyAsync()
    {
        var client = _httpClientFactory.CreateClient();
        var endpoint = BuildDaprHttpEndpoint("/v1.0/metadata");
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);

        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {
            }

            await Task.Delay(500);
        }

        return false;
    }

    private static bool IsCronClosed(string body)
    {
        return body.Contains("cron is closed", StringComparison.OrdinalIgnoreCase);
    }
}
