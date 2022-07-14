// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class UpdateCronJobDomainEventHandler
{
    private readonly QuartzUtils _quartzUtils;

    public UpdateCronJobDomainEventHandler(QuartzUtils quartzUtils)
    {
        _quartzUtils = quartzUtils;
    }

    [EventHandler]
    public async Task UpdateCronJobAsync(UpdateCronJobDomainEvent @event)
    {
        if(@event.Request.ScheduleType == ScheduleTypes.Cron && !string.IsNullOrWhiteSpace(@event.Request.CronExpression) && @event.Request.Enabled)
        {
            await _quartzUtils.RegisterCronJob<StartSchedulerJobQuartzJob>(@event.Request.JobId, @event.Request.CronExpression);
        }
        else
        {
            await _quartzUtils.RemoveCronJob(@event.Request.JobId);
        }
    }
}
