// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;

public class SchedulerTask : AuditAggregateRoot<Guid, Guid>, ISoftDelete
{
    private SchedulerJob _job = new();

    public int RunCount { get; private set; }

    public long RunTime { get; private set; }

    public TaskRunStatuses TaskStatus { get; private set; }

    public DateTimeOffset SchedulerStartTime { get; private set; }

    public DateTimeOffset? TaskRunStartTime { get; private set; }

    public DateTimeOffset? TaskRunEndTime { get; private set; }

    public Guid JobId { get; private set; }

    public SchedulerJob Job => _job;

    public bool IsDeleted { get; private set; }

    public SchedulerTask(Guid jobId)
    {
        JobId = jobId;
        SchedulerStartTime = DateTimeOffset.Now;
    }

    public void TaskStart()
    {
        RunCount++;
        TaskRunStartTime = DateTimeOffset.Now;
        TaskStatus = TaskRunStatuses.Running;
    }

    public void TaskEnd(TaskRunStatuses taskStatus)
    {
        TaskStatus = taskStatus;
        TaskRunEndTime = DateTimeOffset.Now;
        RunTime = Convert.ToInt64((TaskRunStartTime!.Value - TaskRunEndTime.Value).TotalSeconds);
    }
}
