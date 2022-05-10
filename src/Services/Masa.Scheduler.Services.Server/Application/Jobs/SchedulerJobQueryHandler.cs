// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobQueryHandler
{
    readonly ISchedulerJobRepository _schedulerJobRepository;
    public SchedulerJobQueryHandler(ISchedulerJobRepository schedulerJobRepository)
    {
        _schedulerJobRepository = schedulerJobRepository;
    }

    [EventHandler]
    public async Task JobListHandleAsync(SchedulerJobQuery query)
    {
        query.Result = await _schedulerJobRepository.GetListAsync();
    }
}
