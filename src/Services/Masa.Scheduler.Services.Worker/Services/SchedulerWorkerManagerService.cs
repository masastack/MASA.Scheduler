// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Services;

public class SchedulerWorkerManagerService : ServiceBase
{
    public SchedulerWorkerManagerService(IServiceCollection services) : base(services, ConstStrings.SCHEDULER_WORKER_MANAGER_API)
    {
        MapPost(MonitorServerOnlineAsync);
        MapGet(OnlineAsync);
        MapGet(ListAsync);
        MapGet(Heartbeat);
        MapPost(StartTask);
        MapPost(StopTask);
    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(SchedulerServerOnlineIntegrationEvent))]
    public async Task MonitorServerOnlineAsync([FromServices] SchedulerWorkerManager workerManager, SchedulerServerOnlineIntegrationEvent @event)
    {
        await workerManager.MonitorHandler(@event);
    }

    public async Task<IResult> OnlineAsync([FromServices] SchedulerWorkerManager workerManager)
    {
        await workerManager.Online();
        return Results.Ok(); 
    }

    public IResult ListAsync([FromServices] SchedulerWorkerManager workerManager)
    {
        return Results.Ok(workerManager.ServiceList);
    }

    public IResult Heartbeat()
    {
        return Results.Ok("success");
    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(StartTaskIntegrationEvent))]
    public async Task StartTask([FromServices] SchedulerWorkerManager workerManager, StartTaskIntegrationEvent @event)
    {
        await workerManager.EnqueueTask(@event);
    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(StopTaskIntegrationEvent))]
    public async Task StopTask([FromServices] SchedulerWorkerManager workerManager, StopTaskIntegrationEvent @event)
    {
        await workerManager.StopTaskAsync(@event);
    }
}
