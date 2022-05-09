// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs
{
    public class JobQueryHandler
    {
        readonly IJobRepository _jobRepository;
        public JobQueryHandler(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        [EventHandler]
        public async Task JobListHandleAsync(JobQuery query)
        {
            var actorId = new ActorId(Guid.NewGuid().ToString());
            var actor = ActorProxy.Create<IJobActor>(actorId, nameof(JobActor));
            query.Result = await actor.GetListAsync();
        }
    }
}