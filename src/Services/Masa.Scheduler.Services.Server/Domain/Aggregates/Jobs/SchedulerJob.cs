// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs;

public class SchedulerJob : AuditAggregateRoot<Guid, Guid>, ISoftDelete
{
    private SchedulerJobRunDetail _jobRunDetail = null!;

    private List<SchedulerTask> _schedulerTasks = new();

    private SchedulerJobAppConfig? _jobAppConfig;

    private SchedulerJobDaprServiceInvocationConfig? _daprServiceInvocationConfig;
 
    private SchedulerJobHttpConfig? _httpConfig;

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

    public bool Enabled { get; private set; }

    public Guid BelongTeamId { get; private set; }

    public int BelongProjectId { get; private set; }

    public string Origin { get; private set; } = string.Empty;

    public SchedulerJobRunDetail RunDetail { get => _jobRunDetail; private set => _jobRunDetail = value; }

    public SchedulerJobAppConfig? JobAppConfig { get => _jobAppConfig; private set => _jobAppConfig = value; }

    public SchedulerJobDaprServiceInvocationConfig? DaprServiceInvocationConfig { get => _daprServiceInvocationConfig; private set => _daprServiceInvocationConfig = value; }

    public SchedulerJobHttpConfig? HttpConfig { get => _httpConfig; private set => _httpConfig = value; }

    public IReadOnlyCollection<SchedulerTask> SchedulerTasks => _schedulerTasks;

    public bool IsDeleted { get; private set; }

    public SchedulerJob(JobTypes jobType, string origin)
    {
        JobType = jobType;
        Origin = origin;
    }

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
        int failedRetryCount,
        string description,
        Guid belongTeamId,
        int belongProjectId,
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
        FailedRetryCount = failedRetryCount;
        Description = description;
        Enabled = true;
        BelongProjectId = belongProjectId;
        BelongTeamId = belongTeamId;
    }

    public void UpdateJob(
        string name,
        string owner,
        bool isAlertException,
        ScheduleTypes scheduleType,
        RoutingStrategyTypes routingStrategy,
        ScheduleBlockStrategyTypes scheduleBlockStrategy,
        ScheduleExpiredStrategyTypes scheduleExpiredStrategy,
        RunTimeoutStrategyTypes runTimeoutStrategy,
        int runTimeoutSecond,
        FailedStrategyTypes failedStrategy,
        int failedRetryInterval,
        int failedRetryCount,
        string description)
    {
        Name = name;
        Owner = owner;
        IsAlertException = isAlertException;
        ScheduleType = scheduleType;
        RoutingStrategy = routingStrategy;
        ScheduleBlockStrategy = scheduleBlockStrategy;
        ScheduleExpiredStrategy = scheduleExpiredStrategy;
        RunTimeoutStrategy = runTimeoutStrategy;
        RunTimeoutSecond = runTimeoutSecond;
        FailedStrategy = failedStrategy;
        FailedRetryInterval = failedRetryInterval;
        FailedRetryCount = failedRetryCount;
        Description = description;
    }

    public void UpdateRunDetail(TaskRunStatuses taskRunStatus)
    {
        RunDetail.UpdateJobRunDetail(taskRunStatus);
    }

    public void CreateRunDetail()
    {
        RunDetail = new(Id);
    }

    public void SetEnabled()
    {
        Enabled = true;
    }

    public void SetDisabled()
    {
        Enabled = false;
    }

    public void SetJobAppConfig(int jobAppId, string jobEntryAssembly, string jobEntryMethod, string jobParams, string version)
    {
        JobAppConfig = new (Id, jobAppId, jobEntryAssembly, jobEntryMethod, jobParams, version);
    }

    public void SetHttpConfig(HttpMethods httpMethod, string requestUrl, string httpParameter, string httpHeader, string httpBody, HttpVerifyTypes httpVerifyType, string verityContent)
    {
        HttpConfig = new (Id, httpMethod, requestUrl, httpParameter, httpHeader, httpBody, httpVerifyType, verityContent);
    }

    public void SetDaprServiceInvocationConfig(int daprServiceAppId, string methodName, HttpMethods httpMethod, string data)
    {
        DaprServiceInvocationConfig = new(Id, daprServiceAppId, methodName, httpMethod, data);
    }
}
