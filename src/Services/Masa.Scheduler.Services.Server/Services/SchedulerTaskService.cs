// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class SchedulerTaskService : ServiceBase
{
    public SchedulerTaskService(IServiceCollection services) : base(services, ConstStrings.SCHEDULER_TASK_API)
    {
        MapGet(ListAsync, string.Empty);
        MapPut(StartAsync);
        MapPut(StopAsync);
        MapDelete(RemoveAsync, string.Empty);
        MapPost(StartWaitingTask);
    }

    public async Task<IResult> ListAsync(IEventBus eventBus, [FromQuery] Guid jobId, [FromQuery] TaskRunStatus? filterStatus, [FromQuery] string? origin, [FromQuery] JobQueryTimeTypes? queryTimeType, [FromQuery] DateTimeOffset? queryStartTime, [FromQuery] DateTimeOffset? queryEndTime, [FromQuery] int page, [FromQuery] int pageSize)
    {
        var request = new SchedulerTaskListRequest()
        {
            JobId = jobId,
            FilterStatus = filterStatus ?? 0,
            Origin = origin ?? "",
            QueryTimeType = queryTimeType ?? 0,
            QueryStartTime = queryStartTime,
            QueryEndTime = queryEndTime,
            Page = page,
            PageSize = pageSize
        };

        var query = new SchedulerTaskQuery(request);
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result);
    }

    [Topic(ConstStrings.PUB_SUB_NAME, nameof(StartWaitingTaskIntergrationEvent))]
    public async Task StartWaitingTask([FromServices] IEventBus eventBus, StartWaitingTaskIntergrationEvent @event)
    {
        var request = new StartSchedulerTaskRequest()
        {
            TaskId = @event.TaskId,
            OperatorId = @event.OperatorId
        };
        var comman = new StartSchedulerTaskCommand(request);
        await eventBus.PublishAsync(comman);
    }

    public async Task<IResult> StartAsync(IEventBus eventBus, [FromBody] StartSchedulerTaskRequest request)
    {
        var comman = new StartSchedulerTaskCommand(request);
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }

    public async Task<IResult> StopAsync(IEventBus eventBus, [FromBody] StopSchedulerTaskRequest request)
    {
        var comman = new StopSchedulerTaskCommand(request);
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }

    public async Task<IResult> RemoveAsync(IEventBus eventBus, [FromBody] RemoveSchedulerTaskRequest request)
    {
        var comman = new RemoveSchedulerTaskCommand(request);
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }
}
