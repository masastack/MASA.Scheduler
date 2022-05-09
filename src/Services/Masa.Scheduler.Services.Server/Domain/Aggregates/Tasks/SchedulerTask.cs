// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks
{
    public class SchedulerTask : AuditAggregateRoot<Guid, Guid>, ISoftDelete
    {
        private Job _job = new();

        public SchedulerTask()
        {

        }

        public int RunCount { get; private set; }

        public long RunTime { get; private set; }

        public TaskRunStatus TaskStatus { get; private set; }

        public DateTimeOffset TaskRunStartTime { get; private set; }

        public DateTimeOffset TaskRunEndTime { get; private set; }

        public Guid JobId { get; private set; }

        public Job Job => _job;

        public bool IsDeleted { get; set; }
    }
}
