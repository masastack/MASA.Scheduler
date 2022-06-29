// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobDaprServiceInvocationConfig : ValueObject
{
    [JsonInclude]
    public string DaprServiceIdentity { get; private set; } = string.Empty;

    [JsonInclude]
    public string MethodName { get; private set; } = string.Empty;

    [JsonInclude]
    public HttpMethods HttpMethod { get; private set; }

    [JsonInclude]
    public string Data { get; private set; } = string.Empty;

    public void SetConfig(string methodName, HttpMethods httpMethod, string data, string daprServiceIdentity)
    {
        MethodName = methodName;
        HttpMethod = httpMethod;
        Data = data;
        DaprServiceIdentity = daprServiceIdentity;
    }

    protected override IEnumerable<object> GetEqualityValues()
    {
        yield return MethodName;
        yield return HttpMethod;
        yield return Data;
        yield return DaprServiceIdentity;
    }
}
