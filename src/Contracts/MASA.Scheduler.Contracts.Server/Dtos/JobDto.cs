// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace MASA.Scheduler.Contracts.Server.Dtos
{
    public class JobDto
    {
        public string Name { get; private set; } = string.Empty;

        public string Principal { get; private set; } = string.Empty;

        public bool IsAlertException { get; private set; }

        public int AlertMessageTemplate { get; private set; }

        public ScheduleTypes ScheduleType { get; private set; }

        public JobTypes JobType { get; private set; }

        public RoutingStrategyTypes RoutingStrategy { get; private set; }

        public ScheduleExpiredStrategyTypes ScheduleExpiredStrategy { get; private set; }

        public ScheduleBlockStrategyTypes ScheduleBlockStrategy { get; private set; }

        public RunTimeoutStrategyTypes RunTimeoutStrategy { get; private set; }

        public int RunTimeoutSecond { get; private set; }

        public FailedStrategyTypes FailedStrategy { get; private set; }

        public int FailedRetryInterval { get; private set; }

        public int FailedRetryCount { get; private set; }

        public string Description { get; private set; } = string.Empty;

        public int Status { get; private set; }

        public Guid BelongTeamId { get; private set; }

        public int BelongProjectId { get; private set; }

    }
}