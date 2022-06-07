// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;

public class SchedulerTask : FullAggregateRoot<Guid, Guid>
{
    public int RunCount { get; private set; }

    /// <summary>
    /// Task run use total time (second)
    /// </summary>
    public long RunTime { get; private set; }

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

    public SchedulerTask(Guid jobId, string origin, Guid operatorId)
    {
        JobId = jobId;
        Origin = origin;
        OperatorId = operatorId;
    }

    public void TaskSchedule(Guid operatorId)
    {
        SchedulerTime = DateTimeOffset.Now;
        OperatorId = operatorId;
        TaskStatus = TaskRunStatus.Running;
        TaskRunStartTime = DateTimeOffset.MinValue;
        TaskRunEndTime = DateTimeOffset.MinValue;
    }

    public void SetWorkerHost(string workerHost)
    {
        WorkerHost = workerHost;
    }

    public void TaskStart()
    {
        RunCount++;
        TaskRunStartTime = DateTimeOffset.Now;
        TaskStatus = TaskRunStatus.Running;
        RunTime = 0;
    }

    public void TaskEnd(TaskRunStatus taskStatus, string message)
    {
        TaskStatus = taskStatus;
        TaskRunEndTime = DateTimeOffset.Now;
        Message = message;
        RunTime = Convert.ToInt64((TaskRunEndTime - TaskRunStartTime).TotalSeconds);
    }
}
