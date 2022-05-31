// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobDaprServiceInvocationConfig : ValueObject
{
    [JsonInclude]
    public int DaprServiceAppId { get; private set; }

    [JsonInclude]
    public string MethodName { get; private set; } = string.Empty;

    [JsonInclude]
    public HttpMethods HttpMethod { get; private set; }

    [JsonInclude]
    public string Data { get; private set; } = string.Empty;

    public void SetConfig(int daprServiceAppId, string methodName, HttpMethods httpMethod, string data)
    {
        DaprServiceAppId = daprServiceAppId;
        MethodName = methodName;
        HttpMethod = httpMethod;
        Data = data;
    }

    protected override IEnumerable<object> GetEqualityValues()
    {
        yield return DaprServiceAppId;
        yield return MethodName;
        yield return HttpMethod;
        yield return Data;
    }
}
