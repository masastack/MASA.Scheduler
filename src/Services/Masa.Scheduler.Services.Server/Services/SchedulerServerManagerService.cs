// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class SchedulerServerManagerService : ServiceBase
{
    public SchedulerServerManagerService(IServiceCollection services) : base(services, ConstStrings.SCHEDULER_SERVER_MANAGER_API)
    {
        MapGet(OnlineAsync);
        MapGet(GetWorkerListAsync);
        MapGet(Heartbeat);
        MapPost(NotifyTaskRunResultAsync);
        MapPost(MonitorTaskStartAsync);
    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(NotifyTaskStartIntegrationEvent))]
    public async Task MonitorTaskStartAsync([FromServices] IEventBus eventBus, NotifyTaskStartIntegrationEvent @event)
    {
        var command = new NotifyTaskStartCommand(new NotifyTaskStartRequest() { TaskId = @event.TaskId });
        await eventBus.PublishAsync(command);
    }

    public async Task<IResult> OnlineAsync([FromServices] SchedulerServerManager serverManager)
    {
        await serverManager.Online();
        return Results.Ok();
    }

    public IResult GetWorkerListAsync([FromServices] SchedulerServerManagerData data)
    {
        return Results.Ok(data.ServiceList);
    }

    public IResult Heartbeat()
    {
        return Results.Ok("success");
    }
    
    [Topic(ConstStrings.PUB_SUB_NAME, nameof(NotifyTaskRunResultIntegrationEvent))]
    public async Task NotifyTaskRunResultAsync([FromServices] IEventBus eventBus, NotifyTaskRunResultIntegrationEvent @event)
    {
        var command = new NotifySchedulerTaskRunResultCommand(new NotifySchedulerTaskRunResultRequest() 
        {
            Status = @event.Status,
            TaskId = @event.TaskId
        });
        await eventBus.PublishAsync(command);
    }
}
