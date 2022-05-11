// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobCommandHandler
{
    private readonly SchedulerJobDomainService _domainService;

    public SchedulerJobCommandHandler(SchedulerJobDomainService domainService)
    {
        _domainService = domainService;
    }

    [EventHandler(Order = 1)]
    public async Task CreateHandleAsync(AddSchedulerJobCommand command)
    {
        await _domainService.CreateJobAsync();
        //you work
        await Task.CompletedTask;
    }
}