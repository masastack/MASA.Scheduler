// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;

public class DaprServiceInvocationTaskHanlder : ITaskHandler
{
    private readonly ILogger<DaprServiceInvocationTaskHanlder> _logger;

    private readonly DaprClient _daprClient;

    public DaprServiceInvocationTaskHanlder(DaprClient daprClient, ILogger<DaprServiceInvocationTaskHanlder> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<TaskRunStatus> RunTask(Guid taskId, SchedulerJobDto jobDto, DateTimeOffset excuteTime, CancellationToken token)
    {
        if (jobDto.DaprServiceInvocationConfig is null)
        {
            throw new UserFriendlyException("DaprServiceInvocationConfig is required in Dapr Service Invocation Task");
        }

        //todo add excuteTime parameter to data;

        var runStatus = TaskRunStatus.Failure;

        try
        {
            await _daprClient.InvokeMethodAsync(HttpUtils.ConvertHttpMethod(jobDto.DaprServiceInvocationConfig.HttpMethod), jobDto.DaprServiceInvocationConfig.DaprServiceIdentity, jobDto.DaprServiceInvocationConfig.MethodName, jobDto.DaprServiceInvocationConfig.Data, token);

            runStatus = TaskRunStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DaprServiceInvocationTaskHanlder Run Task Error");
        }

        return runStatus;
    }
}
