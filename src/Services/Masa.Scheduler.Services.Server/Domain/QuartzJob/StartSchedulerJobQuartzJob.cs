// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.QuartzJob;

public class StartSchedulerJobQuartzJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMasaStackConfig _masaStackConfig;

    public StartSchedulerJobQuartzJob(IServiceProvider serviceProvider, IMasaStackConfig masaStackConfig)
    {
        _serviceProvider = serviceProvider;
        _masaStackConfig = masaStackConfig;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobId = context.JobDetail.JobDataMap[ConstStrings.JOB_ID];
        var environment = context.JobDetail.JobDataMap[IsolationConsts.ENVIRONMENT];
        var env = environment?.ToString() ?? _masaStackConfig.Environment;

        if (jobId == null)
        {
            return;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var multiEnvironmentSetter = scope.ServiceProvider.GetRequiredService<IMultiEnvironmentSetter>();
        multiEnvironmentSetter.SetEnvironment(env);
        var eventBus = scope.ServiceProvider.GetRequiredService<IDomainEventBus>();

        var @event = new StartJobDomainEvent(new StartSchedulerJobRequest()
        {
            JobId = new Guid(jobId.ToString()!),
            OperatorId = Guid.Empty,
            ExcuteTime = context.ScheduledFireTimeUtc ?? DateTimeOffset.UtcNow
        });

        await eventBus.PublishAsync(@event);
    }
}
