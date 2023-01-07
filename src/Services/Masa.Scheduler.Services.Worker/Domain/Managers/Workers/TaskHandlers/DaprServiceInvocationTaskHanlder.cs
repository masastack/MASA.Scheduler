// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;

public class DaprServiceInvocationTaskHanlder : ITaskHandler
{
    private readonly ILogger<DaprServiceInvocationTaskHanlder> _logger;

    private readonly DaprClient _daprClient;

    private readonly SchedulerLogger _schedulerLogger;

    public DaprServiceInvocationTaskHanlder(DaprClient daprClient, ILogger<DaprServiceInvocationTaskHanlder> logger, SchedulerLogger schedulerLogger)
    {
        _daprClient = daprClient;
        _logger = logger;
        _schedulerLogger = schedulerLogger;
    }

    public async Task<TaskRunStatus> RunTask(Guid taskId, SchedulerJobDto jobDto, DateTimeOffset excuteTime, CancellationToken token)
    {
        if (jobDto.DaprServiceInvocationConfig is null)
        {
            throw new UserFriendlyException("DaprServiceInvocationConfig is required in Dapr Service Invocation Task");
        }

        var runStatus = TaskRunStatus.Failure;

        object? requestObj;

        try
        {
            requestObj = JsonSerializer.Deserialize<dynamic>(jobDto.DaprServiceInvocationConfig.Data);
        }
        catch
        {
            requestObj = jobDto.DaprServiceInvocationConfig.Data;
        }

        string methodName;

        if (jobDto.DaprServiceInvocationConfig.MethodName.Contains('?'))
        {
            methodName = jobDto.DaprServiceInvocationConfig.MethodName + "&";
        }
        else
        {
            methodName = jobDto.DaprServiceInvocationConfig.MethodName + "?";
        }

        methodName += $"taskId={taskId}&excuteTime={System.Web.HttpUtility.UrlEncode(excuteTime.ToString(), System.Text.Encoding.UTF8)}";

        var appId = jobDto.DaprServiceInvocationConfig.DaprServiceIdentity;

        if (!string.IsNullOrWhiteSpace(jobDto.DaprServiceInvocationConfig.Namespace))
        {
            appId += $".{jobDto.DaprServiceInvocationConfig.Namespace}";
        }

        try
        {
            await _daprClient.InvokeMethodAsync(HttpUtils.ConvertHttpMethod(jobDto.DaprServiceInvocationConfig.HttpMethod), appId, methodName, requestObj, token);
            runStatus = TaskRunStatus.Success;
        }
        catch (Exception ex)
        {
            _schedulerLogger.LogError(ex, "DaprServiceInvocation run error", WriterTypes.Job, taskId, jobDto.Id);
            throw;
        }

        return runStatus;
    }
}
