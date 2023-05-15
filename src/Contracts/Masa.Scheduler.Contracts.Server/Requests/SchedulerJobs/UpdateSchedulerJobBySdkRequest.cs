// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Requests.SchedulerJobs;

public class UpdateSchedulerJobBySdkRequest
{
    public string Name { get; set; } = string.Empty;

    public JobTypes JobType { get; set; }

    public string CronExpression { get; set; } = string.Empty;

    public string JobIdentity { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ScheduleExpiredStrategyTypes ScheduleExpiredStrategy { get; set; } = ScheduleExpiredStrategyTypes.Ignore;

    public ScheduleBlockStrategyTypes ScheduleBlockStrategy { get; set; } = ScheduleBlockStrategyTypes.Parallel;

    public RunTimeoutStrategyTypes RunTimeoutStrategy { get; set; } = RunTimeoutStrategyTypes.IgnoreTimeout;

    public int RunTimeoutSecond { get; set; }

    public int FailedRetryInterval { get; set; }

    public int FailedRetryCount { get; set; }

    public SchedulerJobAppConfigDto? JobAppConfig { get; set; }

    public SchedulerJobHttpConfigDto? HttpConfig { get; set; }

    public SchedulerJobDaprServiceInvocationConfigDto? DaprServiceInvocationConfig { get; set; }

    public string NotifyUrl { get; set; } = string.Empty;
}
