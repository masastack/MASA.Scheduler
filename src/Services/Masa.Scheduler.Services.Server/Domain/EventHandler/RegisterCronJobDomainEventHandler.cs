// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class RegisterCronJobDomainEventHandler
{
    private readonly QuartzUtils _quartzUtils;

    public RegisterCronJobDomainEventHandler(QuartzUtils quartzUtils)
    {
        _quartzUtils = quartzUtils;
    }

    [EventHandler]
    public async Task RegisterCronJobAsync(RegisterCronJobDomainEvent @event)
    {
        if(@event.Request.Data.ScheduleType == ScheduleTypes.Cron && !string.IsNullOrEmpty(@event.Request.Data.CronExpression))
        {
            await _quartzUtils.RegisterCronJob<StartSchedulerJobQuartzJob>(@event.Request.Data.Id, @event.Request.Data.CronExpression);
        }
    }
}
