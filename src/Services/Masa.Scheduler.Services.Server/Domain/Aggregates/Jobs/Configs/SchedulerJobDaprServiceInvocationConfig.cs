// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobDaprServiceInvocationConfig
{
    public int DaprServiceAppId { get; private set; }

    public string MethodName { get; private set; } = string.Empty;

    public HttpMethods HttpMethod { get; private set; }

    public string Data { get; private set; } = string.Empty;

    public SchedulerJobDaprServiceInvocationConfig(int daprServiceAppId, string methodName, HttpMethods httpMethod, string data)
    {
        DaprServiceAppId = daprServiceAppId;
        MethodName = methodName;
        HttpMethod = httpMethod;
        Data = data;
    }
}
