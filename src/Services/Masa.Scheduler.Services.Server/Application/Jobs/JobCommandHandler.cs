// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class JobCommandHandler
{
    private readonly JobDomainService _domainService;

    public JobCommandHandler(JobDomainService domainService)
    {
        _domainService = domainService;
    }

    [EventHandler(Order = 1)]
    public async Task CreateHandleAsync(CreateJobCommand command)
    {
        await _domainService.CreateJobAsync();
        //you work
        await Task.CompletedTask;
    }
}