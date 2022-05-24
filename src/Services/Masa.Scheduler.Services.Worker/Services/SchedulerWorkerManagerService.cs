// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Scheduler.Contracts.Server.Infrastructure.Consts;
using Newtonsoft.Json;

namespace Masa.Scheduler.Services.Worker.Services;

public class SchedulerWorkerManagerService : ServiceBase
{
    private readonly SchedulerWorkerManager _workerManager;

    public SchedulerWorkerManagerService(IServiceCollection services, SchedulerWorkerManager workerManager) : base(services, ConstStrings.SCHEDULER_WORKER_MANAGER_API)
    {
        _workerManager = workerManager;
        MapPost(MonitorServerOnlineAsync);
        MapGet(OnlineAsync);
        MapGet(ListAsync);
        MapGet(Heartbeat);
    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(SchedulerServerOnlineIntegrationEvent))]
    public async Task<IResult> MonitorServerOnlineAsync(SchedulerServerOnlineIntegrationEvent @event)
    {
        await _workerManager.MonitorHandler(@event);

        return Results.Ok();
    }

    public async Task<IResult> OnlineAsync()
    {
        await _workerManager.Online();
        return Results.Ok(); 
    }

    public IResult ListAsync()
    {
        return Results.Ok(SchedulerWorkerManager.ServiceList);
    }

    public IResult Heartbeat()
    {
        return Results.Ok("success");
    }

    public async Task<IResult> StartTask(IEventBus eventBus, StartTaskRequest request)
    {
        var command = new StartTaskCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    public async Task<IResult> StopTask(IEventBus eventBus, StopTaskRequest request)
    {
        var command = new StopTaskCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }
}
