// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Server.Services;

public class SchedulerJobService : ServiceBase
{
    public SchedulerJobService(IServiceCollection services) : base(services, "api/job")
    {
        MapGet(ListAsync);
        MapPost(AddAsync);
        MapPut(UpdateAsync);
        MapDelete(DeleteAsync);
    }


    public async Task<IResult> ListAsync(IEventBus eventBus, [FromBody] SchedulerJobListRequest reqeuset)
    {
        var query = new SchedulerJobQuery(reqeuset);
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
