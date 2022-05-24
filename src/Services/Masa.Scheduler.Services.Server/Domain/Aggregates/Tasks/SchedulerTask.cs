﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;

public class SchedulerTask : AuditAggregateRoot<Guid, Guid>, ISoftDelete
{
    public int RunCount { get; private set; }

    public long RunTime { get; private set; }

    public TaskRunStatuses TaskStatus { get; private set; }

    public DateTimeOffset SchedulerStartTime { get; private set; }

    public DateTimeOffset TaskRunStartTime { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset TaskRunEndTime { get; private set; } = DateTimeOffset.MinValue;

    public Guid JobId { get; private set; }

    public SchedulerJob Job { get; set; } = null!;

    public bool IsDeleted { get; private set; }

    public string Origin { get; private set; }

    public string WorkerHost { get; private set; } = string.Empty;

    public SchedulerTask(Guid jobId, string origin)
    {
        JobId = jobId;
        Origin = origin;
        SchedulerStartTime = DateTimeOffset.Now;
    }

    public void TaskStart(string workerHost)
    {
        RunCount++;
        TaskRunStartTime = DateTimeOffset.Now;
        TaskStatus = TaskRunStatuses.Running;
        WorkerHost = workerHost;
    }

    public void TaskEnd(TaskRunStatuses taskStatus)
    {
        TaskStatus = taskStatus;
        TaskRunEndTime = DateTimeOffset.Now;
        RunTime = Convert.ToInt64((TaskRunStartTime - TaskRunEndTime).TotalSeconds);
    }
}
