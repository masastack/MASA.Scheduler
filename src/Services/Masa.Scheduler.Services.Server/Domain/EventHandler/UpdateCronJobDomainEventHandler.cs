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
        if(@event.Request.Data.ScheduleType == ScheduleTypes.Cron && !string.IsNullOrWhiteSpace(@event.Request.Data.CronExpression) && @event.Request.Data.Enabled)
        {
            await _quartzUtils.RegisterCronJob<StartSchedulerJobQuartzJob>(@event.Request.Data.Id, @event.Request.Data.CronExpression);
        }
        else
        {
            await _quartzUtils.RemoveCronJob(@event.Request.Data.Id);
        }
    }
}
