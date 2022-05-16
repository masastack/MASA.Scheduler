// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class SchedulerJobDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public bool IsAlertException { get; set; }

    public ScheduleTypes ScheduleType { get; set; }

    public JobTypes JobType { get; set; }

    public RoutingStrategyTypes RoutingStrategy { get; set; }

    public ScheduleExpiredStrategyTypes ScheduleExpiredStrategy { get; set; }

    public ScheduleBlockStrategyTypes ScheduleBlockStrategy { get; set; }

    public RunTimeoutStrategyTypes RunTimeoutStrategy { get; set; }

    public int RunTimeoutSecond { get; set; }

    public FailedStrategyTypes FailedStrategy { get; set; }

    public int FailedRetryInterval { get; set; }

    public int FailedRetryCount { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public Guid BelongTeamId { get; set; }

    public int BelongProjectId { get; set; }

    public string Origin { get; set; } = string.Empty;

    public DateTimeOffset LastRunTime { get; set; } = DateTimeOffset.MinValue;

    public TaskRunStatuses LastRunStatus { get; set; }

    public SchedulerJobAppConfigDto? JobAppConfig { get; set; }

    public SchedulerJobHttpConfigDto? HttpConfig { get; set; }

    public SchedulerJobDaprServiceInvocationConfigDto? DaprServiceInvocationConfig { get; set; }
}
