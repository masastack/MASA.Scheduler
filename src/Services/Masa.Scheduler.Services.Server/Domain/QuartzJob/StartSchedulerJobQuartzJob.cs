// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.QuartzJob;

public class StartSchedulerJobQuartzJob : IJob
{
    private readonly IDomainEventBus _eventBus;

    public StartSchedulerJobQuartzJob(IDomainEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobId = context.JobDetail.JobDataMap[ConstStrings.JOB_ID];

        if(jobId == null)
        {
            return;
        }

        var @event = new StartJobDomainEvent(new StartSchedulerJobRequest()
        {
            JobId = new Guid(jobId.ToString()!),
            OperatorId = Guid.Empty
        });

        await _eventBus.PublishAsync(@event);
    }
}
