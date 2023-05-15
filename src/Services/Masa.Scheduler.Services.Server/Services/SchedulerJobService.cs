// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Server.Services;

public class SchedulerJobService : ServiceBase
{
    public SchedulerJobService() : base(ConstStrings.SCHEDULER_JOB_API)
    {
    }

    public async Task<IResult> GetListAsync(IEventBus eventBus, [FromQuery] bool isCreatedByManual, [FromQuery] TaskRunStatus? filterStatus, [FromQuery] string? jobName, [FromQuery] JobTypes? jobType, [FromQuery] string? origin, [FromQuery] JobQueryTimeTypes? queryTimeType, [FromQuery] DateTime? queryStartTime, [FromQuery] DateTime? queryEndTime, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string belongProjectIdentity)
    {
        var request = new SchedulerJobListRequest()
        {
            IsCreatedByManual = isCreatedByManual,
            FilterStatus = filterStatus ?? 0,
            JobName = jobName ?? "",
            JobType = jobType ?? 0,
            Origin = origin ?? "",
            QueryTimeType = queryTimeType ?? 0,
            QueryStartTime = queryStartTime,
            QueryEndTime = queryEndTime,
            Page = page,
            PageSize = pageSize,
            BelongProjectIdentity = belongProjectIdentity
        };

        var query = new SchedulerJobQuery(request);
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result);
    }

    public async Task<IResult> AddAsync(IEventBus eventBus, [FromBody] AddSchedulerJobRequest requset)
    {
        var command = new AddSchedulerJobCommand(requset);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    public async Task<IResult> UpdateAsync(IEventBus eventBus, [FromBody] UpdateSchedulerJobRequest requset)
    {
        var command = new UpdateSchedulerJobCommand(requset);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    public async Task<IResult> DeleteAsync(IEventBus eventBus, [FromBody] RemoveSchedulerJobRequest request)
    {
        var command = new RemoveSchedulerJobCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    [RoutePattern(HttpMethod = "Put")]
    public async Task<IResult> ChangeEnableStatusAsync(IEventBus eventBus, [FromBody] ChangeEnabledStatusRequest request)
    {
        var command = new ChangeEnableStatusSchedulerJobCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    [RoutePattern(HttpMethod = "Put")]
    public async Task<IResult> StartJobAsync(IEventBus eventBus, [FromBody] StartSchedulerJobRequest request)
    {
        var command = new StartSchedulerJobCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    [RoutePattern("/addSchedulerJobBySdk", StartWithBaseUri = true, HttpMethod = "Post")]
    public async Task<IResult> AddSchedulerJobBySdkAsync(IEventBus eventBus, [FromBody] AddSchedulerJobBySdkRequest request)
    {
        var command = new AddSchedulerJobBySdkCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok(command.Result.Id);
    }

    [RoutePattern("/{id}/updateSchedulerJobBySdk", StartWithBaseUri = true, HttpMethod = "Put")]
    public async Task<IResult> UpdateSchedulerJobBySdkAsync(IEventBus eventBus, Guid id, [FromBody] UpdateSchedulerJobBySdkRequest request)
    {
        var command = new UpdateSchedulerJobBySdkCommand(id, request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    [RoutePattern("/getSchedulerJobQueryByIdentity", StartWithBaseUri = true, HttpMethod = "Get")]
    public async Task<IResult> GetSchedulerJobQueryByIdentityAsync(IEventBus eventBus, [FromQuery] string jobIdentity, [FromQuery] string projectIdentity)
    {
        var query = new SchedulerJobQueryByIdentity(new GetSchedulerJobByIdentityRequest()
        {
            JobIdentity = jobIdentity,
            ProjectIdentity = projectIdentity
        });
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result);
    }

    [RoutePattern("{id}/upsertAlarm", StartWithBaseUri = true, HttpMethod = "Post")]
    public async Task<IResult> UpsertAlarmRuleAsync(IEventBus eventBus, Guid id, Guid alarmRuleId)
    {
        var command = new UpsertAlarmRuleCommand(id, alarmRuleId);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }
}
