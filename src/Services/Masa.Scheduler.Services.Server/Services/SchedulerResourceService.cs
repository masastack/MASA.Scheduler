// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class SchedulerResourceService : ServiceBase
{
    public SchedulerResourceService(IServiceCollection services) : base(services, ConstStrings.SCHEDULER_RESOURCE_API)
    {
        MapGet(ListAsync, string.Empty);
        MapPost(AddAsync, string.Empty);
        MapPut(UpdateAsync, string.Empty);
        MapDelete(DeleteAsync, string.Empty);
    }

    public async Task<IResult> ListAsync(IEventBus eventBus, [FromQuery] string jobAppIdentity)
    {
        var request = new SchedulerResourceListRequest()
        {
            JobAppIdentity = jobAppIdentity
        };

        var query = new SchedulerResourceListQuery(request);
        await eventBus.PublishAsync(query);
        return Results.Ok(query.Result);
    }

    public async Task<IResult> AddAsync(IEventBus eventBus, [FromBody] AddSchedulerResourceRequest requset)
    {
        var command = new AddSchedulerResourceCommand(requset);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    public async Task<IResult> UpdateAsync(IEventBus eventBus, [FromBody] UpdateSchedulerResourceRequest requset)
    {
        var command = new UpdateSchedulerResourceCommand(requset);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }

    public async Task<IResult> DeleteAsync(IEventBus eventBus, [FromBody] RemoveSchedulerResourceRequest request)
    {
        var command = new RemoveSchedulerResourceCommand(request);
        await eventBus.PublishAsync(command);
        return Results.Ok();
    }
}
