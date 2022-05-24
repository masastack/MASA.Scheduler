// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class SchedulerServerManagerService : ServiceBase
{
    private SchedulerServerManager _serverManager;
    private IEventBus _eventBus;

    public SchedulerServerManagerService(IServiceCollection services, SchedulerServerManager serverManager, IEventBus eventBus) : base(services, ConstStrings.SCHEDULER_SERVER_MANAGER_API)
    {
        _serverManager = serverManager;
        MapPost(MonitorWorkerOnlineAsync);
        MapGet(OnlineAsync);
        MapGet(ListAsync);
        MapGet(Heartbeat);
        MapPost(NotifyTaskRunResultAsync);
        _eventBus = eventBus;
    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(SchedulerWorkerOnlineIntegrationEvent))]
    public async Task MonitorWorkerOnlineAsync(SchedulerWorkerOnlineIntegrationEvent @event)
    {
        await _serverManager.MonitorHandler(@event);
    }

    public async Task<IResult> OnlineAsync()
    {
        await _serverManager.Online();
        return Results.Ok();
    }

    public IResult ListAsync()
    {
        return Results.Ok(SchedulerServerManager.ServiceList);
    }

    public IResult Heartbeat()
    {
        return Results.Ok("success");
    }
    
    [Topic(ConstStrings.PUB_SUB_NAME, nameof(NotifyTaskRunResultIntegrationEvent))]
    public async Task NotifyTaskRunResultAsync(NotifyTaskRunResultIntegrationEvent @event)
    {
        var command = new NotifySchedulerTaskRunResultCommand(new NotifySchedulerTaskRunResultRequest() 
        {
            IsCancel = @event.IsCancel,
            IsSuccess = @event.IsSuccess,
            TaskId = @event.TaskId
        });
        await _eventBus.PublishAsync(command);
    }
}
