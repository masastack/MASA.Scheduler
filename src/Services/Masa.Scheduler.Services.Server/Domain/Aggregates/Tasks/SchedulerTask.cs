// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks;

public class SchedulerTask : FullAggregateRoot<Guid, Guid>
{
    public int RunCount { get; private set; }

    public long RunTime { get; private set; }

    public TaskRunStatuses TaskStatus { get; private set; }

    public DateTimeOffset SchedulerTime { get; private set; }

    public DateTimeOffset TaskRunStartTime { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset TaskRunEndTime { get; private set; } = DateTimeOffset.MinValue;

    public Guid JobId { get; private set; }

    public SchedulerJob Job { get; set; } = null!;

    public string Origin { get; private set; }

    public string WorkerHost { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public Guid RunUserId { get; private set; }

    public SchedulerTask(Guid jobId, string origin, Guid runUserId)
    {
        JobId = jobId;
        Origin = origin;
        RunUserId = runUserId;
    }

    public void TaskSchedule(string workerHost, Guid runUserId)
    {
        SchedulerTime = DateTimeOffset.Now;
        RunUserId = runUserId;
        WorkerHost = workerHost;
    }

    public void TaskStart()
    {
        RunCount++;
        TaskRunStartTime = DateTimeOffset.Now;
        TaskStatus = TaskRunStatuses.Running;
        RunTime = 0;
    }

    public void TaskEnd(TaskRunStatuses taskStatus, string message)
    {
        TaskStatus = taskStatus;
        TaskRunEndTime = DateTimeOffset.Now;
        Message = message;
        RunTime = Convert.ToInt64((TaskRunEndTime - TaskRunStartTime).TotalSeconds);
    }
}
