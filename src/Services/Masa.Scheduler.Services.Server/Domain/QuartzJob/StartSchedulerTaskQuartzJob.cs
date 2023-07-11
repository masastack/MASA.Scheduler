// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.QuartzJob;

public class StartSchedulerTaskQuartzJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public StartSchedulerTaskQuartzJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var taskId = context.JobDetail.JobDataMap[ConstStrings.TASK_ID];
        var environment = context.JobDetail.JobDataMap[IsolationConsts.ENVIRONMENT];
        var env = environment?.ToString() ?? string.Empty;

        if (taskId == null)
        {
            return;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var multiEnvironmentSetter = scope.ServiceProvider.GetRequiredService<IMultiEnvironmentSetter>();
        multiEnvironmentSetter.SetEnvironment(env);
        var eventBus = scope.ServiceProvider.GetRequiredService<IDomainEventBus>();

        var @event = new StartTaskDomainEvent(new StartSchedulerTaskRequest()
        {
            TaskId = new Guid(taskId.ToString()!),
            OperatorId = Guid.Empty
        });

        await eventBus.PublishAsync(@event);
    }
}
