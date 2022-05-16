// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;

public class SchedulerJobRunDetail : Entity<Guid>
{
    public int SuccessCount { get; private set; }

    public int FailureCount { get; private set; }

    public int TimeoutCount { get; private set; }

    public int TimeoutSuccessCount { get; private set; }

    public int TimeoutFailureCount { get; private set; }

    public int TotalRunCount { get; private set; }

    public DateTimeOffset LastRunTime { get; private set; } = DateTimeOffset.MinValue;

    public TaskRunStatuses LastRunStatus { get; private set; }

    public Guid JobId { get; private set; }

    public void UpdateJobRunDetail(TaskRunStatuses status)
    {
        switch (status)
        {
            case TaskRunStatuses.Running:
                TotalRunCount++;
                break;
            case TaskRunStatuses.Success:
                SuccessCount++;
                break;
            case TaskRunStatuses.Failure:
                FailureCount++;
                break;
            case TaskRunStatuses.Timeout:
                TimeoutCount++;
                break;
            case TaskRunStatuses.TimeoutSuccess:
                TimeoutSuccessCount++;
                break;
            case TaskRunStatuses.TimeoutFailure:
                TimeoutFailureCount++;
                break;
        }

        LastRunStatus = status;

        if (status != TaskRunStatuses.Running)
        {
            LastRunTime = DateTimeOffset.Now;
        }
    }
}
