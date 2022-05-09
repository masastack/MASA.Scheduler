// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Server.Services;

public class JobService : ServiceBase
{
    public JobService(IServiceCollection services) : base(services)
    {
        App.MapGet("/job/list", QueryList).Produces<List<Job>>()
            .WithName("GetJobs")
            .RequireAuthorization();
        App.MapPost("/createJob", CreateJob);
    }


    public async Task<IResult> QueryList(JobDomainService jobDomainService)
    {
        var jobs = await jobDomainService.QueryListAsync();
        return Results.Ok(jobs);
    }

    public async Task<IResult> CreateJob(IEventBus eventBus)
    {
        var comman = new CreateJobCommand();
        await eventBus.PublishAsync(comman);
        return Results.Ok();
    }
}
