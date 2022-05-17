﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Server.Services;

public class SchedulerJobService : ServiceBase
{
    public SchedulerJobService(IServiceCollection services) : base(services, "api/job")
    {
        MapGet(ListAsync, string.Empty);
        MapPost(AddAsync, string.Empty);
        MapPut(UpdateAsync, string.Empty);
        MapDelete(DeleteAsync, string.Empty);
    }

    public async Task<IResult> ListAsync(IEventBus eventBus, [FromQuery] bool isCreatedByManual, [FromQuery] TaskRunStatuses? filterStatus, [FromQuery] string? jobName, [FromQuery] JobTypes? jobType, [FromQuery] string? origin, [FromQuery] JobQueryTimeTypes? queryTimeType, [FromQuery] DateTimeOffset? queryStartTime, [FromQuery] DateTimeOffset? queryEndTime, [FromQuery] int page, [FromQuery] int pageSize)
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
            PageSize = pageSize
        };

        var query = new SchedulerJobQuery(request);
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result);
    }

    public async Task<IResult> AddAsync(IEventBus eventBus, [FromBody] AddSchedulerJobRequest reqeuset)
    {
        var comman = new AddSchedulerJobCommand(reqeuset);
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }

    public async Task<IResult> UpdateAsync(IEventBus eventBus, [FromBody] UpdateSchedulerJobRequest reqeuset)
    {
        var comman = new UpdateSchedulerJobCommand(reqeuset);
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }

    public async Task<IResult> DeleteAsync(IEventBus eventBus, [FromQuery] Guid jobId)
    {
        var comman = new RemoveSchedulerJobCommand(jobId);
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }
}