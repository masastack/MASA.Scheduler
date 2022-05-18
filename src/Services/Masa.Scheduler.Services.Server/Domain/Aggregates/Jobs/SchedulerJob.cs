// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;

public class SchedulerJob : AuditAggregateRoot<Guid, Guid>, ISoftDelete
{
    private List<SchedulerTask> _schedulerTasks = new();

    private SchedulerJobAppConfig _jobAppConfig = new();

    private SchedulerJobDaprServiceInvocationConfig _daprServiceInvocationConfig = new();

    private SchedulerJobHttpConfig _httpConfig = new();

    public string Name { get; private set; } = string.Empty;

    public string Owner { get; private set; } = string.Empty;

    public bool IsAlertException { get; private set; }

    public ScheduleTypes ScheduleType { get; private set; }

    public string CronExpression { get; private set; } = string.Empty;

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

    public bool Enabled { get; private set; }

    public Guid BelongTeamId { get; private set; }

    public int BelongProjectId { get; private set; }

    public string Origin { get; private set; } = string.Empty;

    public DateTimeOffset LastScheduleTime { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset LastRunStartTime { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset LastRunEndTime { get; private set; } = DateTimeOffset.MinValue;

    public TaskRunStatuses LastRunStatus { get; private set; }

    public SchedulerJobAppConfig JobAppConfig { get => _jobAppConfig; private set => _jobAppConfig = value; }

    public SchedulerJobDaprServiceInvocationConfig DaprServiceInvocationConfig { get => _daprServiceInvocationConfig; private set => _daprServiceInvocationConfig = value; }

    public SchedulerJobHttpConfig HttpConfig { get => _httpConfig; private set => _httpConfig = value; }

    public IReadOnlyCollection<SchedulerTask> SchedulerTasks => _schedulerTasks;

    public bool IsDeleted { get; private set; }

    public SchedulerJob(JobTypes jobType, string origin)
    {
        JobType = jobType;
        Origin = origin;
        Enabled = true;
    }

    public SchedulerJob(
        string name,
        string owner,
        bool isAlertException,
        ScheduleTypes scheduleType,
        string cronExpression,
        JobTypes jobType,
        RoutingStrategyTypes routingStrategy,
        ScheduleExpiredStrategyTypes scheduleExpiredStrategy,
        RunTimeoutStrategyTypes runTimeoutStrategy,
        int runTimeoutSecond,
        FailedStrategyTypes failedStrategy,
        int failedRetryInterval,
        int failedRetryCount,
        string description,
        Guid belongTeamId,
        int belongProjectId)
    {
        Name = name;
        Owner = owner;
        IsAlertException = isAlertException;
        ScheduleType = scheduleType;
        CronExpression = cronExpression;
        JobType = jobType;
        RoutingStrategy = routingStrategy;
        ScheduleExpiredStrategy = scheduleExpiredStrategy;
        RunTimeoutStrategy = runTimeoutStrategy;
        RunTimeoutSecond = runTimeoutSecond;
        FailedStrategy = failedStrategy;
        FailedRetryInterval = failedRetryInterval;
        FailedRetryCount = failedRetryCount;
        Description = description;
        Enabled = true;
        BelongProjectId = belongProjectId;
        BelongTeamId = belongTeamId;
    }

    public void UpdateJob(SchedulerJobDto dto)
    {
        Name = dto.Name;
        Owner = dto.Owner;
        IsAlertException = dto.IsAlertException;
        ScheduleType = dto.ScheduleType;
        CronExpression = dto.CronExpression;
        RoutingStrategy = dto.RoutingStrategy;
        ScheduleBlockStrategy = dto.ScheduleBlockStrategy;
        ScheduleExpiredStrategy = dto.ScheduleExpiredStrategy;
        RunTimeoutStrategy = dto.RunTimeoutStrategy;
        RunTimeoutSecond = dto.RunTimeoutSecond;
        FailedStrategy = dto.FailedStrategy;
        FailedRetryInterval = dto.FailedRetryInterval;
        FailedRetryCount = dto.FailedRetryCount;
        Description = dto.Description;

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
        LastScheduleTime = scheduleTime;
    }

    public void UpdateLastRunDetail(TaskRunStatuses taskRunStatus)
    {
        LastRunStatus = taskRunStatus;

        switch (taskRunStatus)
        {
            case TaskRunStatuses.Running:
                LastRunStartTime = DateTimeOffset.Now;
                break;
            case TaskRunStatuses.Success:
            case TaskRunStatuses.TimeoutSuccess:
            case TaskRunStatuses.Timeout:
            case TaskRunStatuses.Failure:
                LastRunEndTime = DateTimeOffset.Now;
                break;
        }
    }

    public void ChangeEnableStatus()
    {
        Enabled = !Enabled;
    }

    public void SetJobAppConfig(SchedulerJobAppConfigDto? dto)
    {
        if (dto == null)
        {
            return;
        }
        JobAppConfig ??= new();
        JobAppConfig.SetConfig(dto.JobEntryAssembly, dto.JobEntryMethod, dto.JobParams, dto.Version);
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
        DaprServiceInvocationConfig.SetConfig(dto.DaprServiceAppId, dto.MethodName, dto.HttpMethod, dto.Data);
    }
}
