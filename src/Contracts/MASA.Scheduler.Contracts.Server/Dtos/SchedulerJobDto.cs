// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class SchedulerJobDto
{
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

    public bool IsEnabled { get; set; }

    public Guid BelongTeamId { get; set; }

    public int BelongProjectId { get; set; }

}
