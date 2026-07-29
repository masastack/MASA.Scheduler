// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public class DaprJobsSchedulerBackend : ISchedulerBackend
{
    private const string CronTimeZone = "Asia/Shanghai";

    private readonly DaprJobsClient _daprJobsClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IOptions<SchedulerBackendOptions> _options;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;
    private readonly ILogger<DaprJobsSchedulerBackend> _logger;

    public DaprJobsSchedulerBackend(
        DaprJobsClient daprJobsClient,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IOptions<SchedulerBackendOptions> options,
        IMultiEnvironmentContext multiEnvironmentContext,
        ILogger<DaprJobsSchedulerBackend> logger)
    {
        _daprJobsClient = daprJobsClient;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _options = options;
        _multiEnvironmentContext = multiEnvironmentContext;
        _logger = logger;
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task RegisterCronJob(string environment, Guid jobId, string cron)
    {
        var name = DaprJobsNameHelper.BuildCronName(environment, jobId);
        var payload = new DaprJobPayload
        {
            Type = DaprJobNameType.Cron,
            JobId = jobId,
            Environment = environment,
            CronExpression = cron
        };

        return ScheduleCronJobAsync(name, cron, payload);
    }

    public Task RemoveCronJob(Guid jobId)
    {
        var name = DaprJobsNameHelper.BuildCronName(_multiEnvironmentContext.CurrentEnvironment, jobId);
        return DeleteJobAsync(name, throwOnMissing: true);
    }

    public async Task AddDelayTask(string environment, Guid taskId, Guid jobId, TimeSpan delay)
    {
        var name = DaprJobsNameHelper.BuildRetryName(environment, jobId, taskId);
        var payload = new DaprJobPayload
        {
            Type = DaprJobNameType.Retry,
            JobId = jobId,
            TaskId = taskId,
            Environment = environment
        };

        if (_options.Value.DaprJobs.Overwrite)
        {
            await TryDeleteJobAsync(name);
        }

        var dueTime = DateTimeOffset.UtcNow.Add(delay);
        var result = await ScheduleJobAsync(name, DaprJobSchedule.FromDateTime(dueTime), payload, startingFrom: null, ttl: null);
        if (!result.Success)
        {
            _logger.LogWarning("DaprJobs delay schedule failed. Name: {Name}. Error: {Error}", name, result.Error);
        }
    }

    public Task RemoveDelayTask(Guid taskId, Guid jobId)
    {
        var name = DaprJobsNameHelper.BuildRetryName(_multiEnvironmentContext.CurrentEnvironment, jobId, taskId);
        return DeleteJobAsync(name, throwOnMissing: true);
    }

    public Task<List<DateTimeOffset>> GetCronExecuteTimeByTimeRange(string cron, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        return DaprJobsTimeCalculator.GetCronExecuteTimeByTimeRange(cron, startTime, endTime);
    }

    private async Task DeleteJobAsync(string name, bool throwOnMissing)
    {
        await TryDeleteJobAsync(name, throwOnMissing);
    }

    private async Task TryDeleteJobAsync(string name, bool throwOnMissing = false)
    {
        try
        {
            await _daprJobsClient.DeleteJobAsync(name);
        }
        catch (Exception ex)
        {
            if (!throwOnMissing && DaprJobsExceptionHelper.IsNotFound(ex))
            {
                _logger.LogInformation("Delete Dapr job ignored because job was not found. Name: {Name}", name);
                return;
            }

            _logger.LogError(ex, "Delete Dapr job failed. Name: {Name}", name);
            throw;
        }
    }

    private async Task ScheduleCronJobAsync(string name, string cron, DaprJobPayload payload)
    {
        var candidates = DaprJobsCronExpressionNormalizer.BuildCronCandidates(cron);
        var activationWindow = DaprJobsCronExpressionNormalizer.BuildCronActivationWindow(cron);
        if (candidates.Count == 0)
        {
            throw new UserFriendlyException("CronExpression is empty");
        }

        if (_options.Value.DaprJobs.Overwrite)
        {
            await TryDeleteJobAsync(name);
        }

        string? lastError = null;
        foreach (var candidate in candidates)
        {
            var schedule = BuildDaprCronSchedule(candidate);
            var result = await TryScheduleCronViaHttpAsync(name, schedule, payload, activationWindow.StartingFrom, activationWindow.Ttl);
            if (result.Success)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                lastError = result.Error;
            }
        }

        _logger.LogWarning("DaprJobs schedule candidates failed. Cron: {Cron}. Candidates: {Candidates}", cron, string.Join(" | ", candidates));
        var message = string.IsNullOrWhiteSpace(lastError) ? "DaprJobs schedule failed" : $"DaprJobs schedule failed: {lastError}";
        throw new UserFriendlyException(message);
    }

    private static string BuildDaprCronSchedule(string cron)
    {
        return $"CRON_TZ={CronTimeZone} {cron}";
    }

    private async Task<ScheduleJobResult> TryScheduleCronViaHttpAsync(string name, string schedule, DaprJobPayload payload, DateTimeOffset? startingFrom, DateTimeOffset? ttl)
    {
        try
        {
            var daprHttpEndpoint = DaprEndpointResolver.Resolve(_configuration, "DAPR_HTTP_ENDPOINT", "DAPR_HTTP_PORT", 3500);
            var requestUri = $"{daprHttpEndpoint}/v1.0/jobs/{Uri.EscapeDataString(name)}";
            var requestBody = new Dictionary<string, object?>
            {
                ["schedule"] = schedule,
                ["data"] = payload,
                ["overwrite"] = _options.Value.DaprJobs.Overwrite
            };

            if (startingFrom.HasValue)
            {
                requestBody["dueTime"] = startingFrom.Value.ToString("O");
            }

            if (ttl.HasValue)
            {
                requestBody["ttl"] = ttl.Value.ToString("O");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(requestBody)
            };

            var daprApiToken = Environment.GetEnvironmentVariable("DAPR_API_TOKEN");
            if (!string.IsNullOrWhiteSpace(daprApiToken))
            {
                request.Headers.TryAddWithoutValidation("dapr-api-token", daprApiToken);
            }

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("DaprJobs schedule applied via HTTP: {Schedule}, StartingFrom: {StartingFrom}, Ttl: {Ttl}", schedule, startingFrom, ttl);
                return ScheduleJobResult.SuccessResult();
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("DaprJobs schedule failed via HTTP. StatusCode: {StatusCode}. Error: {Error}", response.StatusCode, error);
            return ScheduleJobResult.Fail(string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DaprJobs schedule failed via HTTP.");
            return ScheduleJobResult.Fail(ex.Message);
        }
    }

    private async Task<ScheduleJobResult> ScheduleJobAsync(string name, DaprJobSchedule schedule, DaprJobPayload payload, DateTimeOffset? startingFrom, DateTimeOffset? ttl)
    {
        try
        {
            ReadOnlyMemory<byte> payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
            await _daprJobsClient.ScheduleJobAsync(name, schedule, payloadBytes, startingFrom, repeats: null, ttl);
            return ScheduleJobResult.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DaprJobs schedule failed via SDK.");
            return ScheduleJobResult.Fail(ex.Message);
        }
    }

}
