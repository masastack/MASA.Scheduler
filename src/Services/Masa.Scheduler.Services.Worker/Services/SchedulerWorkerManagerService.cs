// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Services;

public class SchedulerWorkerManagerService : ServiceBase
{
    private readonly ILogger<SchedulerWorkerManagerService> _logger;
    private readonly SchedulerLogger _schedulerLogger;

    public SchedulerWorkerManagerService(IServiceCollection services, ILogger<SchedulerWorkerManagerService> logger, SchedulerLogger schedulerLogger) : base(services, ConstStrings.SCHEDULER_WORKER_MANAGER_API)
    {
        _logger = logger;
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var serviceId = MD5Utils.Encrypt(EncryptType.Md5, host.HostName);
        MapGet(OnlineAsync);
        MapGet(GetServerListAsync);
        MapGet(Heartbeat);
        MapPost(StartTask).WithTopic(ConstStrings.PUB_SUB_NAME, nameof(StartTaskIntegrationEvent) + serviceId);
        MapPost(StopTask).WithTopic(ConstStrings.PUB_SUB_NAME, nameof(StopTaskIntegrationEvent) + serviceId);
        _schedulerLogger = schedulerLogger;
    }

    public async Task<IResult> OnlineAsync([FromServices] SchedulerWorkerManager workerManager)
    {
        await workerManager.Online();
        return Results.Ok(); 
    }

    public IResult GetServerListAsync([FromServices] SchedulerWorkerManagerData data)
    {
        return Results.Ok(data.ServiceList);
    }

    public IResult Heartbeat()
    {
        return Results.Ok("success");
    }

    public async Task StartTask([FromServices] SchedulerWorkerManager workerManager, [FromServices] IIntegrationEventBus eventBus, StartTaskIntegrationEvent @event)
    {
        if(@event.TaskId == Guid.Empty || @event.Job == null)
        {
            var notifyEvent = new NotifyTaskRunResultIntegrationEvent()
            {
                TaskId = @event.TaskId,
                Status = TaskRunStatus.Failure,
                Message = "StartTask Error, Job is null"
            };

            await eventBus.PublishAsync(notifyEvent);
            return;
        }

        _schedulerLogger.LogInformation($"Receive Start Task Event", WriterTypes.Worker, @event.TaskId, @event.Job.Id);

        await workerManager.EnqueueTask(@event);
    }

    public async Task StopTask([FromServices] SchedulerWorkerManager workerManager, StopTaskIntegrationEvent @event)
    {
        await workerManager.StopTaskAsync(@event);
    }
}
