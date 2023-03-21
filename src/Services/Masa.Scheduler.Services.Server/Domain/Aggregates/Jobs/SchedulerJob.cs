// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;

public class SchedulerJob : FullAggregateRoot<Guid, Guid>
{
    private List<SchedulerTask> _schedulerTasks = new();

    private SchedulerJobAppConfig _jobAppConfig = new();

    private SchedulerJobDaprServiceInvocationConfig _daprServiceInvocationConfig = new();

    private SchedulerJobHttpConfig _httpConfig = new();

    public string Name { get; private set; } = string.Empty;

    public string Owner { get; private set; } = string.Empty;

    public Guid OwnerId { get; set; }

    public bool IsAlertException { get; private set; }

    public ScheduleTypes ScheduleType { get; private set; }

    public string CronExpression { get; private set; } = string.Empty;

    public string JobIdentity { get; private set; } = string.Empty;

    public JobTypes JobType { get; private set; }

    public RoutingStrategyTypes RoutingStrategy { get; private set; }

    public string SpecifiedWorkerHost { get; set; } = string.Empty;

    public ScheduleExpiredStrategyTypes ScheduleExpiredStrategy { get; private set; }

    public ScheduleBlockStrategyTypes ScheduleBlockStrategy { get; private set; }

    public RunTimeoutStrategyTypes RunTimeoutStrategy { get; private set; }

    public int RunTimeoutSecond { get; private set; }

    public FailedStrategyTypes FailedStrategy { get; private set; }

    public int FailedRetryInterval { get; private set; }

    public int FailedRetryCount { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public bool Enabled { get; private set; }

    public Guid BelongTeamId { get; private set; }

    public string BelongProjectIdentity { get; private set; } = string.Empty;

    public string Origin { get; private set; } = string.Empty;

    public string NotifyUrl { get; private set; } = string.Empty;

    public Guid AlarmRuleId { get; private set; }

    public DateTimeOffset UpdateExpiredStrategyTime { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset LastScheduleTime { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset LastRunStartTime { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset LastRunEndTime { get; private set; } = DateTimeOffset.MinValue;

    public TaskRunStatus LastRunStatus { get; private set; }

    public SchedulerJobAppConfig JobAppConfig { get => _jobAppConfig; private set => _jobAppConfig = value; }

    public SchedulerJobDaprServiceInvocationConfig DaprServiceInvocationConfig { get => _daprServiceInvocationConfig; private set => _daprServiceInvocationConfig = value; }

    public SchedulerJobHttpConfig HttpConfig { get => _httpConfig; private set => _httpConfig = value; }

    public IReadOnlyCollection<SchedulerTask> SchedulerTasks => _schedulerTasks;

    public SchedulerJob(JobTypes jobType, string origin)
    {
        JobType = jobType;
        Origin = origin;
        Enabled = true;
    }

    public SchedulerJob(
        string name,
        string owner,
        Guid ownerId,
        bool isAlertException,
        ScheduleTypes scheduleType,
        string cronExpression,
        JobTypes jobType,
        RoutingStrategyTypes routingStrategy,
        string specifiedWorkerHost,
        ScheduleExpiredStrategyTypes scheduleExpiredStrategy,
        RunTimeoutStrategyTypes runTimeoutStrategy,
        int runTimeoutSecond,
        FailedStrategyTypes failedStrategy,
        int failedRetryInterval,
        int failedRetryCount,
        string description,
        Guid belongTeamId,
        string belongProjectIdentity,
        string jobIdentity)
    {
        Name = name;
        Owner = owner;
        OwnerId = ownerId;
        IsAlertException = isAlertException;
        ScheduleType = scheduleType;
        CronExpression = cronExpression;
        JobType = jobType;
        RoutingStrategy = routingStrategy;
        SpecifiedWorkerHost = specifiedWorkerHost;
        ScheduleExpiredStrategy = scheduleExpiredStrategy;
        RunTimeoutStrategy = runTimeoutStrategy;
        RunTimeoutSecond = runTimeoutSecond;
        FailedStrategy = failedStrategy;
        FailedRetryInterval = failedRetryInterval;
        FailedRetryCount = failedRetryCount;
        Description = description;
        Enabled = true;
        BelongProjectIdentity = belongProjectIdentity;
        BelongTeamId = belongTeamId;
        JobIdentity = jobIdentity;
    }

    public void UpdateJob(SchedulerJobDto dto)
    {
        Name = dto.Name;
        Owner = dto.Owner;
        OwnerId = dto.OwnerId;
        IsAlertException = dto.IsAlertException;
        ScheduleType = dto.ScheduleType;
        CronExpression = dto.CronExpression;
        RoutingStrategy = dto.RoutingStrategy;
        SpecifiedWorkerHost = dto.SpecifiedWorkerHost;
        ScheduleBlockStrategy = dto.ScheduleBlockStrategy;
        ScheduleExpiredStrategy = dto.ScheduleExpiredStrategy;
        RunTimeoutStrategy = dto.RunTimeoutStrategy;
        RunTimeoutSecond = dto.RunTimeoutSecond;
        FailedStrategy = dto.FailedStrategy;
        FailedRetryInterval = dto.FailedRetryInterval;
        FailedRetryCount = dto.FailedRetryCount;
        Description = dto.Description;
        UpdateExpiredStrategyTime = dto.UpdateExpiredStrategyTime;
        NotifyUrl = dto.NotifyUrl;
        AlarmRuleId = dto.AlarmRuleId;

        switch (dto.JobType)
        {
            case JobTypes.Http:
                SetHttpConfig(dto.HttpConfig);
                break;
            case JobTypes.JobApp:
                SetJobAppConfig(dto.JobAppConfig);
                break;
            case JobTypes.DaprServiceInvocation:
                SetDaprServiceInvocationConfig(dto.DaprServiceInvocationConfig);
                break;
            default:
                throw new UserFriendlyException("Job type error");
        }
    }

    public void UpdateLastScheduleTime(DateTimeOffset scheduleTime)
    {
        LastScheduleTime = scheduleTime == DateTimeOffset.MinValue ? DateTimeOffset.Now : scheduleTime;
    }

    public void UpdateLastRunDetail(TaskRunStatus taskRunStatus)
    {
        LastRunStatus = taskRunStatus;

        switch (taskRunStatus)
        {
            case TaskRunStatus.Running:
                LastRunStartTime = DateTimeOffset.Now;
                break;
            case TaskRunStatus.Success:
            case TaskRunStatus.TimeoutSuccess:
            case TaskRunStatus.Timeout:
                NotifyJobStatus(JobNotifyStatus.Timeout);
                break;
            case TaskRunStatus.Failure:
                LastRunEndTime = DateTimeOffset.Now;
                NotifyJobStatus(JobNotifyStatus.Failure);
                break;
        }
    }

    public void NotifyJobStatus(JobNotifyStatus status)
    {
        if (!string.IsNullOrEmpty(NotifyUrl))
        {
            AddDomainEvent(new NotifyJobStatusDomainEvent(Id, NotifyUrl, status));
        }
    }
    
    public void ChangeEnableStatus(bool enabled)
    {
        Enabled = enabled;
        NotifyJobStatus(enabled ? JobNotifyStatus.Enabled : JobNotifyStatus.Disable);
    }

    public void SetJobAppConfig(SchedulerJobAppConfigDto? dto)
    {
        if (dto == null)
        {
            return;
        }
        JobAppConfig ??= new();
        JobAppConfig.SetConfig(dto.JobAppIdentity, dto.JobEntryAssembly, dto.JobEntryClassName, dto.JobParams, dto.Version);
    }

    public void SetHttpConfig(SchedulerJobHttpConfigDto? dto)
    {
        if (dto == null)
        {
            return;
        }

        HttpConfig ??= new();
        HttpConfig.SetConfig(dto.HttpMethod, dto.RequestUrl, dto.HttpParameters, dto.HttpHeaders, dto.HttpBody, dto.HttpVerifyType, dto.VerifyContent);
    }

    public void SetDaprServiceInvocationConfig(SchedulerJobDaprServiceInvocationConfigDto? dto)
    {
        if (dto == null)
        {
            return;
        }
        DaprServiceInvocationConfig ??= new();
        DaprServiceInvocationConfig.SetConfig(dto.MethodName, dto.Namespace, dto.HttpMethod, dto.Data, dto.DaprServiceIdentity);
    }
}
