// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Server.Services;

public class SchedulerJobService : ServiceBase
{
    public SchedulerJobService(IServiceCollection services) : base(services)
    {
        App.MapGet("/job/list", QueryList).Produces<List<SchedulerJob>>()
            .WithName("GetJobs")
            .RequireAuthorization();
        App.MapPost("/createJob", CreateJob);
    }


    public async Task<IResult> QueryList(SchedulerJobDomainService jobDomainService)
    {
        var jobs = await jobDomainService.QueryListAsync();
        return Results.Ok(jobs);
    }

    public async Task<IResult> CreateJob(IEventBus eventBus)
    {
        var comman = new CreateSchedulerJobCommand();
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }
}
