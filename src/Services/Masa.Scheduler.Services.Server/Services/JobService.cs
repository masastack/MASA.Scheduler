﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Server.Services
{
    public class JobService : ServiceBase
    {
        public JobService(IServiceCollection services) : base(services, "api/job")
        {
            MapGet(ListAsync);
            MapPost(CreateAsync);
        }


        public async Task<IResult> ListAsync(JobDomainService jobDomainService)
        {
            var jobs = await jobDomainService.QueryListAsync();
            return Results.Ok(jobs);
        }

        public async Task<IResult> CreateAsync(IEventBus eventBus)
        {
            var comman = new JobCreateCommand();
            await eventBus.PublishAsync(comman);
            return Results.Ok();
        }
    }
}