// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class SchedulerTaskService : ServiceBase
{
    public SchedulerTaskService() : base(ConstStrings.SCHEDULER_TASK_API)
    {
    }

    public async Task<IResult> GetListAsync(IEventBus eventBus, [FromQuery] Guid jobId, [FromQuery] TaskRunStatus? filterStatus, [FromQuery] string? origin, [FromQuery] JobQueryTimeTypes? queryTimeType, [FromQuery] DateTime? queryStartTime, [FromQuery] DateTime? queryEndTime, [FromQuery] int page, [FromQuery] int pageSize)
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

    [RoutePattern(HttpMethod = "Put")]
    public async Task<IResult> StartAsync(IEventBus eventBus, [FromBody] StartSchedulerTaskRequest request)
    {
        var comman = new StartSchedulerTaskCommand(request);
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }

    [RoutePattern(HttpMethod = "Put")]
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

    public async Task<IResult> NotifyRunResultBySdkAsync([FromServices] IEventBus eventBus, [FromBody] NotifySchedulerTaskRunResultBySdkRequest request)
    {
        var command = new NotifySchedulerTaskRunResultBySdkCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }
}
