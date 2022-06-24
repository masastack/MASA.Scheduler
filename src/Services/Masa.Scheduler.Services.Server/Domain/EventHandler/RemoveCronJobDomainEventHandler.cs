// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class RemoveCronJobDomainEventHandler
{
    private readonly QuartzUtils _quartzUtils;

    public RemoveCronJobDomainEventHandler(QuartzUtils quartzUtils)
    {
        _quartzUtils = quartzUtils;
    }

    [EventHandler]
    public async Task RemoveCronJobAsync(RemoveCronJobDomainEvent @event)
    {
        await _quartzUtils.RemoveCronJob(@event.Request.JobId);
    }
}
