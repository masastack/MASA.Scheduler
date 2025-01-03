﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;

public class SchedulerTask : FullAggregateRoot<Guid, Guid>
{
    public int RunCount { get; private set; }

    /// <summary>
    /// Task run use total time (second)
    /// </summary>
    public long RunTime { get; private set; }

    /// <summary>
    /// TSC's Trace Id
    /// </summary>
    public string TraceId { get; private set; } = string.Empty;

    public TaskRunStatus TaskStatus { get; private set; }

    public DateTimeOffset SchedulerTime { get; private set; }

    public DateTimeOffset TaskRunStartTime { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset TaskRunEndTime { get; private set; } = DateTimeOffset.MinValue;

    public Guid JobId { get; private set; }

    public SchedulerJob Job { get; set; } = null!;

    public string Origin { get; private set; }

    public string WorkerHost { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public Guid OperatorId { get; private set; }

    public SchedulerTask(Guid jobId, string origin, Guid operatorId, DateTimeOffset schedulerTime)
    {
        JobId = jobId;
        Origin = origin;
        OperatorId = operatorId;
        UpdateTaskSchedulerTime(schedulerTime);
    }

    public void UpdateTaskSchedulerTime(DateTimeOffset excuteTime)
    {
        SchedulerTime = excuteTime == DateTimeOffset.MinValue ? DateTimeOffset.UtcNow : excuteTime;
    }

    public void TaskSchedule(Guid operatorId)
    {
        OperatorId = operatorId;
        TaskStatus = TaskRunStatus.Running;
        TaskRunStartTime = DateTimeOffset.MinValue;
        TaskRunEndTime = DateTimeOffset.MinValue;
    }

    public void SetWorkerHost(string workerHost)
    {
        WorkerHost = workerHost;
    }

    public void Wait()
    {
        TaskStatus = TaskRunStatus.WaitToRun;
    }

    public void Discard()
    {
        TaskStatus = TaskRunStatus.Failure;
    }

    public void TaskStart()
    {
        RunCount++;
        TaskRunStartTime = DateTimeOffset.UtcNow;
        TaskStatus = TaskRunStatus.Running;
        TaskRunEndTime = DateTimeOffset.MinValue;
        RunTime = 0;
    }

    public void TaskEnd(TaskRunStatus taskStatus, string message)
    {
        TaskStatus = taskStatus;

        if (taskStatus != TaskRunStatus.Timeout && taskStatus != TaskRunStatus.Ignore)
        {
            TaskRunEndTime = DateTimeOffset.UtcNow;
        }

        Message = message;

        if (taskStatus != TaskRunStatus.Ignore)
        {
            RunTime = Convert.ToInt64((TaskRunEndTime - TaskRunStartTime).TotalSeconds);
        }
    }

    public void TaskStartError(string message)
    {
        var now = DateTimeOffset.UtcNow;
        RunCount++;
        TaskRunStartTime = now;
        TaskRunEndTime = now;
        TaskStatus = TaskRunStatus.Failure;
        RunTime = 0;
        Message = message;
    }

    public void SetTraceId(string? traceId)
    {
        TraceId = string.IsNullOrEmpty(traceId) ? TraceId : traceId;
    }
}
