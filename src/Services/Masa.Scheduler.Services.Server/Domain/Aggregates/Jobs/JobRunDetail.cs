// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs
{
    public class JobRunDetail: Entity<Guid>
    {
        public int SuccessCount { get; private set; }

        public int FailedCount { get; private set; }

        public int TimeoutCount { get; private set; }

        public DateTimeOffset LastRunTime { get; private set; }

        public TaskRunStatus LastRunStatus { get; private set; }

        public Guid JobId { get; private set; }
    }
}
