// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class SchedulerServerManagerService : ServiceBase
{
    public SchedulerServerManagerService() : base(ConstStrings.SCHEDULER_SERVER_MANAGER_API)
    {

    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(NotifyTaskStartIntegrationEvent))]
    public async Task MonitorTaskStartAsync([FromServices] IEventBus eventBus, NotifyTaskStartIntegrationEvent @event)
    {
        var command = new NotifyTaskStartCommand(new NotifyTaskStartRequest() { TaskId = @event.TaskId });
        await eventBus.PublishAsync(command);
    }

    [RoutePattern(Pattern = "/online", StartWithBaseUri = true, HttpMethod = "Get")]
    public async Task<IResult> OnlineAsync([FromServices] SchedulerServerManager serverManager)
    {
        await serverManager.Online();
        return Results.Ok();
    }

    public IResult GetWorkerListAsync([FromServices] SchedulerServerManagerData data)
    {
        return Results.Ok(data.ServiceList);
    }

    public IResult GetHeartbeat()
    {
        return Results.Ok("success");
    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(NotifyTaskRunResultIntegrationEvent))]
    public async Task NotifyTaskRunResultAsync([FromServices] IEventBus eventBus, NotifyTaskRunResultIntegrationEvent @event)
    {
        var command = new NotifySchedulerTaskRunResultCommand(new NotifySchedulerTaskRunResultRequest()
        {
            Status = @event.Status,
            TaskId = @event.TaskId,
            Message = @event.Message,
            TraceId = @event.TraceId,
        });
        await eventBus.PublishAsync(command);
    }
}
