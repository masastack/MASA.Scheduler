// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Managers.Workers;

public class SchedulerWorkerManager : BaseSchedulerManager<ServerModel, SchedulerWorkerOnlineIntegrationEvent, SchedulerServerOnlineIntegrationEvent>
{
    private ILogger<SchedulerWorkerManager> _logger;

    private readonly SchedulerWorkerManagerData _data;

    public SchedulerWorkerManager(IDistributedCacheClientFactory cacheClientFactory, IDistributedCacheClient redisCacheClient, IServiceProvider serviceProvider, IIntegrationEventBus eventBus, ILogger<SchedulerWorkerManager> logger, IHttpClientFactory httpClientFactory, SchedulerWorkerManagerData data) : base(cacheClientFactory, redisCacheClient, serviceProvider, eventBus, httpClientFactory, data)
    {
        _data = data;
        _logger = logger;
    }

    protected override string HeartbeatApi { get; set; } = $"{ConstStrings.SCHEDULER_WORKER_MANAGER_API}/heartbeat";

    protected override ILogger<BaseSchedulerManager<ServerModel, SchedulerWorkerOnlineIntegrationEvent, SchedulerServerOnlineIntegrationEvent>> Logger => _logger;
    
    public Task EnqueueTask(StartTaskIntegrationEvent @event)
    {
        if (@event.Job is null)
        {
            throw new UserFriendlyException("Job cannot be null");
        }

        if (@event.ServiceId == _data.ServiceId)
        {
            _data.TaskQueue.Enqueue(new TaskRunModel() { Job = @event.Job, TaskId = @event.TaskId });
        }

        return Task.CompletedTask;
    }

    public override async Task OnManagerStartAsync()
    {
        try
        {
            await base.OnManagerStartAsync();

            await ProcessTaskRun();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "OnManagerStartAsync");
        }
    }

    private Task ProcessTaskRun()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    if (_data.TaskQueue.Count == 0)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    if (_data.TaskQueue.TryDequeue(out var task))
                    {
                        if (_data.StopTask.Any(p => p == task.TaskId))
                        {
                            _data.StopTask.Remove(task.TaskId);
                            continue;
                        }

                        await StartTaskAsync(task.TaskId, task.Job);
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, "ProcessTaskRunError");
                }
            }
        });

        return Task.CompletedTask;
    }

    public async Task StartTaskAsync(Guid taskId, SchedulerJobDto job)
    {
        var cts = new CancellationTokenSource();

        cts.Token.Register(() =>
        {
            if(_data.InternalCancellationTokenSources.TryGetValue(taskId, out cts))
            {
                cts.Cancel();
            }

            var @event = new NotifyTaskRunResultIntegrationEvent()
            {
                Status = TaskRunStatus.Stop,
                TaskId = taskId
            };

            _data.TaskCancellationTokenSources.Remove(taskId, out _);
            _data.InternalCancellationTokenSources.Remove(taskId, out _);
            EventBus.PublishAsync(@event);
            EventBus.CommitAsync();
        });

        _data.TaskCancellationTokenSources.TryAdd(taskId, cts);

        await NofityTaskStart(taskId);

        switch (job.JobType)
        {
            case JobTypes.JobApp:
                break;
            case JobTypes.Http:
                _ = Task.Run(async () =>
                {
                    var innerCts = new CancellationTokenSource();
                    _data.InternalCancellationTokenSources.TryAdd(taskId, innerCts);
                    await RunHttpTask(taskId, job, innerCts.Token);

                }, cts.Token);
                break;
            case JobTypes.DaprServiceInvocation:
                break;
            default:
                break;
        }
    }

    public Task StopTaskAsync(StopTaskIntegrationEvent @event)
    {
        if(@event.ServiceId == _data.ServiceId)
        {
            if (_data.TaskCancellationTokenSources.TryGetValue(@event.TaskId, out var task))
            {
                task.Cancel();
            }
            else
            {
                _data.StopTask.Add(@event.TaskId);
            }
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
            Status = isSuccess ? TaskRunStatus.Success : TaskRunStatus.Failure
        };

        await EventBus.PublishAsync(@event);
        await EventBus.CommitAsync();
    }

    private async Task NofityTaskStart(Guid taskId)
    {
        var @event = new NotifyTaskStartIntegrationEvent()
        {
            TaskId = taskId
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

    private static void AddHttpHeader(HttpClient client, List<KeyValuePair<string, string>> httpHeaders)
    {
        foreach (var header in httpHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    private static Uri GetRequestUrl(string requestUrl, List<KeyValuePair<string, string>> httpParameters)
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
