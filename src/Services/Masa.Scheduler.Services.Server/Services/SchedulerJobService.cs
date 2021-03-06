// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.
namespace Masa.Scheduler.Services.Server.Server.Services;

public class SchedulerJobService : ServiceBase
{
    public SchedulerJobService(IServiceCollection services) : base(services, ConstStrings.SCHEDULER_JOB_API)
    {
        MapGet(ListAsync, string.Empty);
        MapPost(AddAsync, string.Empty);
        MapPut(UpdateAsync, string.Empty);
        MapDelete(DeleteAsync, string.Empty);
        MapPut(ChangeEnableStatusAsync);
        MapPut(StartJobAsync);
        MapPost(AddSchedulerJobBySdkAsync);
    }

    public async Task<IResult> ListAsync(IEventBus eventBus, [FromQuery] bool isCreatedByManual, [FromQuery] TaskRunStatus? filterStatus, [FromQuery] string? jobName, [FromQuery] JobTypes? jobType, [FromQuery] string? origin, [FromQuery] JobQueryTimeTypes? queryTimeType, [FromQuery] DateTimeOffset? queryStartTime, [FromQuery] DateTimeOffset? queryEndTime, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string belongProjectIdentity)
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

    public async Task<IResult> ChangeEnableStatusAsync(IEventBus eventBus, [FromBody] ChangeEnabledStatusRequest request)
    {
        var command = new ChangeEnableStatusSchedulerJobCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    public async Task<IResult> StartJobAsync(IEventBus eventBus, [FromBody] StartSchedulerJobRequest request)
    {
        var command = new StartSchedulerJobCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }
    
    public async Task<IResult> AddSchedulerJobBySdkAsync(IEventBus eventBus, [FromBody] AddSchedulerJobBySdkRequest request)
    {
        var command = new AddSchedulerJobBySdkCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok(command.Result.Id);
    }
}
