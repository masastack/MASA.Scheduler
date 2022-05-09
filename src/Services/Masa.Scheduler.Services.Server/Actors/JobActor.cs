// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Actors;

public class JobActor : Actor, IJobActor
{

    readonly IJobRepository _jobRepository;

    public JobActor(ActorHost host, IJobRepository jobRepository) : base(host)
    {
        _jobRepository = jobRepository;
    }

    public async Task<List<Job>> GetListAsync()
    {
        var data = await _jobRepository.GetListAsync();
        return data;
    }
}
