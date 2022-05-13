// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;

public class SchedulerJob : AuditAggregateRoot<Guid, Guid>, ISoftDelete
{
    private SchedulerJobRunDetail _jobRunDetail = new();
    private List<SchedulerTask> _schedulerTasks = new();

    public string Name { get; private set; } = string.Empty;

    public string Owner { get; private set; } = string.Empty;

    public bool IsAlertException { get; private set; }

    public ScheduleTypes ScheduleType { get; private set; }

    public JobTypes JobType { get; private set; }

    public RoutingStrategyTypes RoutingStrategy { get; private set; }

    public ScheduleExpiredStrategyTypes ScheduleExpiredStrategy { get; private set; }

    public ScheduleBlockStrategyTypes ScheduleBlockStrategy { get; private set; }

    public RunTimeoutStrategyTypes RunTimeoutStrategy { get; private set; }

    public int RunTimeoutSecond { get; private set; }

    public FailedStrategyTypes FailedStrategy { get; private set; }

    public int FailedRetryInterval { get; private set; }

    public int FailedRetryCount { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public Guid BelongTeamId { get; private set; }

    public int BelongProjectId { get; private set; }

    public Guid ResourceId { get; private set; }

    public string MainFunc { get; private set; } = string.Empty;

    public SchedulerJobRunDetail RunDetail { get => _jobRunDetail; private set => _jobRunDetail = value; }

    public IReadOnlyCollection<SchedulerTask> SchedulerTasks => _schedulerTasks;

    public bool IsDeleted { get; private set; }

    public SchedulerJob(
        string name,
        string owner,
        bool isAlertException,
        ScheduleTypes scheduleType,
        JobTypes jobType,
        RoutingStrategyTypes routingStrategy,
        ScheduleExpiredStrategyTypes scheduleExpiredStrategy,
        RunTimeoutStrategyTypes runTimeoutStrategy,
        int runTimeoutSecond,
        FailedStrategyTypes failedStrategy,
        int failedRetryInterval,
        string description,
        bool isEnabled,
        Guid belongTeamId,
        int belongProjectId,
        Guid resourseId,
        string mainFunc)
    {
        Name = name;
        Owner = owner;
        IsAlertException = isAlertException;
        ScheduleType = scheduleType;
        JobType = jobType;
        RoutingStrategy = routingStrategy;
        ScheduleExpiredStrategy = scheduleExpiredStrategy;
        RunTimeoutStrategy = runTimeoutStrategy;
        RunTimeoutSecond = runTimeoutSecond;
        FailedStrategy = failedStrategy;
        FailedRetryInterval = failedRetryInterval;
        Description = description;
        IsEnabled = isEnabled;
        BelongProjectId = belongProjectId;
        BelongTeamId = belongTeamId;
        ResourceId = resourseId;
        MainFunc = mainFunc;
    }

    public void UpdateJob(
        string name,
        string owner,
        bool isAlertException,
        ScheduleTypes scheduleType,
        JobTypes jobType,
        RoutingStrategyTypes routingStrategy,
        ScheduleExpiredStrategyTypes scheduleExpiredStrategy,
        RunTimeoutStrategyTypes runTimeoutStrategy,
        int runTimeoutSecond,
        FailedStrategyTypes failedStrategy,
        int failedRetryInterval,
        string description,
        bool isEnabled)
    {
        Name = name;
        Owner = owner;
        IsAlertException = isAlertException;
        ScheduleType = scheduleType;
        JobType = jobType;
        RoutingStrategy = routingStrategy;
        ScheduleExpiredStrategy = scheduleExpiredStrategy;
        RunTimeoutStrategy = runTimeoutStrategy;
        RunTimeoutSecond = runTimeoutSecond;
        FailedStrategy = failedStrategy;
        FailedRetryInterval = failedRetryInterval;
        Description = description;
        IsEnabled = isEnabled;
    }

    public void UpdateRunDetail(TaskRunStatuses taskRunStatus)
    {
        RunDetail.UpdateJobRunDetail(taskRunStatus);
    }
}
