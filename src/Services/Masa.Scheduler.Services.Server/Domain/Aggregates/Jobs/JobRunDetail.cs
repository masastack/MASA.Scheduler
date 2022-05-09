// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs
{
    public class JobRunDetail: Entity<Guid>
    {
        public int SuccessCount { get; private set; }

        public int FailureCount { get; private set; }

        public int TimeoutCount { get; private set; }

        public int TimeoutSuccessCount { get; private set; }

        public int TimeoutFailureCount { get; private set; }

        public int TotalRunCount { get; private set; }

        public DateTimeOffset LastRunTime { get; private set; }

        public TaskRunStatus LastRunStatus { get; private set; }

        public Guid JobId { get; private set; }

        public void UpdateJobRunDetail(TaskRunStatus status)
        {
            switch (status)
            {
                case TaskRunStatus.Running:
                    TotalRunCount++;
                    break;
                case TaskRunStatus.Success:
                    SuccessCount++;
                    break;
                case TaskRunStatus.Failure:
                    FailureCount++;
                    break;
                case TaskRunStatus.Stopped:
                    break;
                case TaskRunStatus.Timeout:
                    TimeoutCount++;
                    break;
                case TaskRunStatus.TimeoutSuccess:
                    TimeoutSuccessCount++;
                    break;
                case TaskRunStatus.TimeoutFailure:
                    TimeoutFailureCount++;
                    break;
            }
            LastRunStatus = status;
            LastRunTime = DateTimeOffset.Now;
        }
    }
}
