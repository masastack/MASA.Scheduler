// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks
{
    public class SchedulerTask : AuditAggregateRoot<Guid, Guid>, ISoftDelete
    {
        private Job _job = new();

        public int RunCount { get; private set; }

        public long RunTime { get; private set; }

        public TaskRunStatus TaskStatus { get; private set; }

        public DateTimeOffset SchedulerStartTime { get; private set; }

        public DateTimeOffset? TaskRunStartTime { get; private set; }

        public DateTimeOffset? TaskRunEndTime { get; private set; }

        public Guid JobId { get; private set; }

        public Job Job => _job;

        public bool IsDeleted { get; set; }

        public SchedulerTask(Guid jobId)
        {
            JobId = jobId;
            SchedulerStartTime = DateTimeOffset.Now;
        }

        public void TaskStart()
        {
            RunCount++;
            TaskRunStartTime = DateTimeOffset.Now;
            TaskStatus = TaskRunStatus.Running;
        }

        public void TaskEnd(TaskRunStatus taskStatus)
        {
            TaskStatus = taskStatus;
            TaskRunEndTime = DateTimeOffset.Now;
            RunTime = Convert.ToInt64((TaskRunStartTime!.Value - TaskRunEndTime.Value).TotalSeconds);
        }
    }
}
