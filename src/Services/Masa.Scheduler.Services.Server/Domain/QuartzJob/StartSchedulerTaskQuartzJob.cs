// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.QuartzJob;

public class StartSchedulerTaskQuartzJob : IJob
{
    private readonly IDomainEventBus _eventBus;

    public StartSchedulerTaskQuartzJob(IDomainEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var taskId = context.JobDetail.JobDataMap[ConstStrings.TASK_ID];
        
        if(taskId == null)
        {
            return;
        }

        var @event = new StartTaskDomainEvent(new StartSchedulerTaskRequest()
        {
            TaskId = new Guid(taskId.ToString()!),
            OperatorId = Guid.Empty
        });

        await _eventBus.PublishAsync(@event);
    }
}
