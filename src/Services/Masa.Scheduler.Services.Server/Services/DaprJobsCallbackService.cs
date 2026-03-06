// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class DaprJobsCallbackService : ServiceBase
{
    public DaprJobsCallbackService() : base("job")
    {
        RouteHandlerBuilder = builder =>
        {
            builder.AllowAnonymous();
        };
    }

    [RoutePattern("{name}", StartWithBaseUri = true, HttpMethod = "Post")]
    public async Task<IResult> ReceiveAsync(
        IEventBus eventBus,
        IOptions<SchedulerBackendOptions> options,
        ILogger<DaprJobsCallbackService> logger,
        IMultiEnvironmentSetter multiEnvironmentSetter,
        string name,
        HttpRequest request)
    {
        var useDaprJobs = string.Equals(options.Value.Backend, SchedulerBackendType.DaprJobs, StringComparison.OrdinalIgnoreCase);
        if (!useDaprJobs)
        {
            logger.LogInformation("DaprJobs callback ignored because backend is {Backend}. Name: {Name}", options.Value.Backend, name);
            return Results.Ok();
        }

        if (!DaprJobsNameHelper.TryParse(name, out var nameInfo))
        {
            return Results.BadRequest("Invalid job name");
        }

        multiEnvironmentSetter.SetEnvironment(nameInfo.Environment);

        var payload = await ReadPayloadAsync(request);

        switch (nameInfo.Type)
        {
            case DaprJobNameType.Cron:
                var jobRequest = new StartSchedulerJobRequest
                {
                    JobId = nameInfo.JobId,
                    OperatorId = Guid.Empty,
                    ExcuteTime = payload?.ExecuteTime ?? DateTimeOffset.UtcNow
                };
                await eventBus.PublishAsync(new StartJobDomainEvent(jobRequest));
                break;
            case DaprJobNameType.Retry:
                var taskRequest = new StartSchedulerTaskRequest
                {
                    TaskId = nameInfo.TaskId,
                    OperatorId = Guid.Empty,
                    ExcuteTime = payload?.ExecuteTime ?? DateTimeOffset.UtcNow
                };
                await eventBus.PublishAsync(new StartTaskDomainEvent(taskRequest));
                break;
        }

        return Results.Ok();
    }

    private static async Task<DaprJobPayload?> ReadPayloadAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
        {
            return null;
        }

        using var reader = new StreamReader(request.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("data", out var dataElement))
            {
                return dataElement.Deserialize<DaprJobPayload>();
            }

            return root.Deserialize<DaprJobPayload>();
        }
        catch
        {
            return null;
        }
    }
}
