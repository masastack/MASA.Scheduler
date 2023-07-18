// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;

public class HttpTaskHandler : ITaskHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpTaskHandler> _logger;
    private readonly SchedulerLogger _schedulerLogger;

    public HttpTaskHandler(IHttpClientFactory httpClientFactory, ILogger<HttpTaskHandler> logger, SchedulerLogger schedulerLogger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _schedulerLogger = schedulerLogger;
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

        jobDto.HttpConfig.HttpParameters.Add(new("taskId", taskId.ToString()));
        jobDto.HttpConfig.HttpParameters.Add(new("excuteTime", System.Web.HttpUtility.UrlEncode(excuteTime.ToString(), System.Text.Encoding.UTF8)));
        jobDto.HttpConfig.HttpParameters.Add(new("traceId", traceId ?? ""));
        jobDto.HttpConfig.HttpParameters.Add(new("spanId", spanId ?? ""));

        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpUtils.ConvertHttpMethod(jobDto.HttpConfig.HttpMethod),
            RequestUri = HttpUtils.GetRequestUrl(jobDto.HttpConfig.RequestUrl, jobDto.HttpConfig.HttpParameters),
            Content = HttpUtils.ConvertHttpContent(jobDto.HttpConfig.HttpBody)
        };
        //if (traceId.IsNullOrEmpty() == false)
        //{
        //    //client.DefaultRequestHeaders.Add("traceparent", traceId);
        //    //Activity.Current = new ActivitySource("HttpJob").StartActivity();
        //}

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

            if (runSucess != TaskRunStatus.Success && !string.IsNullOrEmpty(response.ReasonPhrase))
            {
                _schedulerLogger.LogError(response.ReasonPhrase, WriterTypes.Worker, taskId, jobDto.Id);
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
}
