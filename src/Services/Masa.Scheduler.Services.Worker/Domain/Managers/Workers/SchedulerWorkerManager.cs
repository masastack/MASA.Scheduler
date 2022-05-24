// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Masa.Scheduler.Services.Worker.Managers.Workers;

public class SchedulerWorkerManager : BaseSchedulerManager<ServerModel, SchedulerWorkerOnlineIntegrationEvent, SchedulerServerOnlineIntegrationEvent>
{
    private readonly Dictionary<Guid, CancellationTokenSource> _taskCancellationTokenSources = new Dictionary<Guid, CancellationTokenSource>();

    private readonly Dictionary<Guid, CancellationTokenSource> _internalCancellationTokenSources = new Dictionary<Guid, CancellationTokenSource>();

    private ILogger<SchedulerWorkerManager> _logger;

    public SchedulerWorkerManager(IDistributedCacheClientFactory cacheClientFactory, IDistributedCacheClient redisCacheClient, IServiceProvider serviceProvider, IIntegrationEventBus eventBus, ILogger<SchedulerWorkerManager> logger, IHttpClientFactory httpClientFactory) : base(cacheClientFactory, redisCacheClient, serviceProvider, eventBus, httpClientFactory)
    {
        _logger = logger;
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_WORKER_MANAGER_API}/heartbeat";

    protected override ILogger<BaseSchedulerManager<ServerModel, SchedulerWorkerOnlineIntegrationEvent, SchedulerServerOnlineIntegrationEvent>> Logger => _logger;

    public Task StartTaskAsync(Guid taskId, SchedulerJobDto jobDto)
    {
        if(jobDto is null)
        {
            throw new UserFriendlyException("Job dto cannot be null");
        }

        var cts = new CancellationTokenSource();

        cts.Token.Register(() =>
        {
            if(_internalCancellationTokenSources.TryGetValue(taskId, out cts))
            {
                cts.Cancel();
            }

            var @event = new NotifyTaskRunResultIntegrationEvent()
            {
                IsCancel = true,
                IsSuccess = false,
                TaskId = taskId
            };

            _taskCancellationTokenSources.Remove(taskId);
            _internalCancellationTokenSources.Remove(taskId);
            EventBus.PublishAsync(@event);
            EventBus.CommitAsync();
        });

        switch (jobDto.JobType)
        {
            case JobTypes.JobApp:
                break;
            case JobTypes.Http:
                var task = new Task(async () =>
                {
                    var innerCts = new CancellationTokenSource();
                    _internalCancellationTokenSources.Add(taskId, innerCts);

                    await RunHttpTask(taskId, jobDto, innerCts.Token);

                }, cts.Token);

                _taskCancellationTokenSources.Add(taskId, cts);
                
                task.Start();
                break;
            case JobTypes.DaprServiceInvocation:
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }

    public Task StopTaskAsync(Guid taskId)
    {
        if(_taskCancellationTokenSources.TryGetValue(taskId, out var task))
        {
            task.Cancel();
        }

        return Task.CompletedTask;
    }

    private async Task RunHttpTask(Guid taskId, SchedulerJobDto jobDto, CancellationToken token)
    {
        if(jobDto.HttpConfig is null)
        {
            throw new UserFriendlyException("HttpConfig is required in Http Task");
        }

        var client = _httpClientFactory.CreateClient();

        AddHttpHeader(client, jobDto.HttpConfig.HttpHeaders);

        var requestMessage = new HttpRequestMessage()
        {
            Method = ConvertHttpMethod(jobDto.HttpConfig.HttpMethod),
            RequestUri = GetRequestUrl(jobDto.HttpConfig.RequestUrl, jobDto.HttpConfig.HttpParameters),
            Content = ConvertHttpContent(jobDto.HttpConfig.HttpBody)
        };

        var response = await client.SendAsync(requestMessage, token);

        var isSucess = false;

        string? content;

        switch (jobDto.HttpConfig.HttpVerifyType)
        {
            case HttpVerifyTypes.StatusCode200:
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    isSucess = true;
                }
                break;
            case HttpVerifyTypes.CustomStatusCode:
                if(string.IsNullOrWhiteSpace(jobDto.HttpConfig.VerifyContent) || response.StatusCode.ToString("d") == jobDto.HttpConfig.VerifyContent)
                {
                    isSucess = true;
                }
                break;
            case HttpVerifyTypes.ContentContains:
                content = await response.Content.ReadAsStringAsync();
                if(string.IsNullOrWhiteSpace(jobDto.HttpConfig.VerifyContent) || content.Contains(jobDto.HttpConfig.VerifyContent))
                {
                    isSucess = true;
                }
                break;
            case HttpVerifyTypes.ContentUnContains:
                content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(jobDto.HttpConfig.VerifyContent) || !content.Contains(jobDto.HttpConfig.VerifyContent))
                {
                    isSucess = true;
                }
                break;
            default:
                isSucess = response.IsSuccessStatusCode;
                break;
        }

        await NotifyTaskRunResult(isSucess, taskId);
    }

    private async Task NotifyTaskRunResult(bool isSuccess, Guid taskId)
    {
        var @event = new NotifyTaskRunResultIntegrationEvent()
        {
            TaskId = taskId,
            IsSuccess = isSuccess
        };

        await EventBus.PublishAsync(@event);
        await EventBus.CommitAsync();
    }

    private static HttpMethod ConvertHttpMethod(Contracts.Server.Infrastructure.Enums.HttpMethods methods)
    {
        switch (methods)
        {
            case Contracts.Server.Infrastructure.Enums.HttpMethods.GET:
                return HttpMethod.Get;
            case Contracts.Server.Infrastructure.Enums.HttpMethods.POST:
                return HttpMethod.Post;
            case Contracts.Server.Infrastructure.Enums.HttpMethods.HEAD:
                return HttpMethod.Head;
            case Contracts.Server.Infrastructure.Enums.HttpMethods.PUT:
                return HttpMethod.Put;
            case Contracts.Server.Infrastructure.Enums.HttpMethods.DELETE:
                return HttpMethod.Delete;
            default:
                throw new UserFriendlyException($"Cannot convert method: {methods}");
        }
    }

    private static void AddHttpHeader(HttpClient client, Dictionary<string,string> httpHeaders)
    {
        foreach (var header in httpHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    private static Uri GetRequestUrl(string requestUrl, Dictionary<string,string> httpParameters)
    {
        var builder = new UriBuilder(requestUrl);

        builder.Query = string.Join("&", httpParameters.Select(p => $"{p.Key}={p.Value}"));

        return builder.Uri;
    }

    private static HttpContent? ConvertHttpContent(string content)
    {
        if(string.IsNullOrEmpty(content))
        {
            return null;
        }

        var contentType = "text/plain";

        if ((content.StartsWith("{") && content.EndsWith("}")) || content.StartsWith("[") && content.EndsWith("]"))
        {
            contentType = "application/json";
        }
        else if (content.StartsWith("<") && content.EndsWith(">"))
        {
            contentType = "application/xml";
        }

        return new StringContent(content, Encoding.UTF8, contentType);
    }
}
