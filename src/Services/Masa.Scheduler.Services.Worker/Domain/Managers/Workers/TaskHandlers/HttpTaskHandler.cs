// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;

public class HttpTaskHandler : ITaskHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpTaskHandler> _logger;
    private readonly SchedulerLogger _schedulerLogger;
    private readonly IMultiEnvironmentContext _multiEnvironmentContext;
    private readonly ICacheContext _cacheContext;
    private readonly IMasaStackConfig _masaStackConfig;

    public HttpTaskHandler(IHttpClientFactory httpClientFactory, ILogger<HttpTaskHandler> logger, SchedulerLogger schedulerLogger, IMultiEnvironmentContext multiEnvironmentContext, ICacheContext cacheContext, IMasaStackConfig masaStackConfig)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _schedulerLogger = schedulerLogger;
        _multiEnvironmentContext = multiEnvironmentContext;
        _cacheContext = cacheContext;
        _masaStackConfig = masaStackConfig;
    }

    public async Task<TaskRunStatus> RunTask(Guid taskId, SchedulerJobDto jobDto, DateTimeOffset excuteTime, string? traceId, string? spanId, CancellationToken token)
    {
        if (jobDto.HttpConfig is null)
        {
            throw new UserFriendlyException("HttpConfig is required in Http Task");
        }

        var client = _httpClientFactory.CreateClient();

        if (jobDto.RunTimeoutSecond > 0)
        {
            client.Timeout = TimeSpan.FromSeconds(jobDto.RunTimeoutSecond);
        }

        HttpUtils.AddHttpHeader(client, jobDto.HttpConfig.HttpHeaders);

        try
        {
            var accessToken = await GetClientCredentialsTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get access token for HTTP request");
        }

        jobDto.HttpConfig.HttpParameters.Add(new("jobId", jobDto.Id.ToString()));
        jobDto.HttpConfig.HttpParameters.Add(new("taskId", taskId.ToString()));
        jobDto.HttpConfig.HttpParameters.Add(new("excuteTime", System.Web.HttpUtility.UrlEncode(excuteTime.ToString(), System.Text.Encoding.UTF8)));
        jobDto.HttpConfig.HttpParameters.Add(new("traceId", traceId ?? ""));
        jobDto.HttpConfig.HttpParameters.Add(new("spanId", spanId ?? ""));
        jobDto.HttpConfig.HttpParameters.Add(new(IsolationConsts.ENVIRONMENT, _multiEnvironmentContext.CurrentEnvironment));

        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpUtils.ConvertHttpMethod(jobDto.HttpConfig.HttpMethod),
            RequestUri = HttpUtils.GetRequestUrl(jobDto.HttpConfig.RequestUrl, jobDto.HttpConfig.HttpParameters),
            Content = HttpUtils.ConvertHttpContent(jobDto.HttpConfig.HttpBody)
        };

        TaskRunStatus runSucess = TaskRunStatus.Failure;

        try
        {
            var response = await client.SendAsync(requestMessage, token);

            string? content;

            switch (jobDto.HttpConfig.HttpVerifyType)
            {
                case HttpVerifyTypes.StatusCode200:
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        runSucess = TaskRunStatus.Success;
                    }
                    break;
                case HttpVerifyTypes.CustomStatusCode:
                    if (string.IsNullOrWhiteSpace(jobDto.HttpConfig.VerifyContent) || response.StatusCode.ToString("d") == jobDto.HttpConfig.VerifyContent)
                    {
                        runSucess = TaskRunStatus.Success;
                    }
                    break;
                case HttpVerifyTypes.ContentContains:
                    content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(jobDto.HttpConfig.VerifyContent) || content.Contains(jobDto.HttpConfig.VerifyContent))
                    {
                        runSucess = TaskRunStatus.Success;
                    }
                    break;
                case HttpVerifyTypes.ContentUnContains:
                    content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(jobDto.HttpConfig.VerifyContent) || !content.Contains(jobDto.HttpConfig.VerifyContent))
                    {
                        runSucess = TaskRunStatus.Success;
                    }
                    break;
                default:
                    runSucess = response.IsSuccessStatusCode ? TaskRunStatus.Success : TaskRunStatus.Failure;
                    break;
            }

            if (runSucess != TaskRunStatus.Success)
            {
                string errorMessage = $"HTTP Status Code: {(int)response.StatusCode} ({response.StatusCode}) - " +
                    (string.IsNullOrEmpty(response.ReasonPhrase) 
                        ? "No additional information provided" 
                        : response.ReasonPhrase);
                _schedulerLogger.LogError(errorMessage, WriterTypes.Worker, taskId, jobDto.Id);
            }
        }
        catch (TimeoutException ex)
        {
            runSucess = TaskRunStatus.Timeout;
            _schedulerLogger.LogError(ex, "HttpRequestTimeout", WriterTypes.Worker, taskId, jobDto.Id);
        }
        catch (Exception ex)
        {
            _schedulerLogger.LogError(ex, "HttpRequestError", WriterTypes.Worker, taskId, jobDto.Id);
            throw;
        }

        return runSucess;
    }

    private async Task<string> GetClientCredentialsTokenAsync()
    {
        var clientId = _masaStackConfig.GetWebId(MasaStackProject.Scheduler);
        var accessToken = await _cacheContext.GetOrSetAsync(ConstStrings.ClientCredentialsTokenKey(clientId),
            async () =>
            {
                var request = new ClientCredentialsTokenRequest
                {
                    Address = _masaStackConfig.GetSsoDomain() + "/connect/token",
                    GrantType = BuildingBlocks.Authentication.OpenIdConnect.Models.Constans.GrantType.CLIENT_CREDENTIALS,
                    ClientId = clientId,
                    Scope = ConstStrings.COMMON_SCOPE
                };

                var client = _httpClientFactory.CreateClient();
                var tokenResponse = await client.RequestClientCredentialsTokenAsync(request);

                var expirationSeconds = Math.Max(tokenResponse.ExpiresIn - 60, 60);
                var cacheEntryOptions = new CacheEntryOptions(TimeSpan.FromSeconds(expirationSeconds));
                return (tokenResponse.AccessToken, cacheEntryOptions);
            }
            );

        return accessToken;
    }
}
