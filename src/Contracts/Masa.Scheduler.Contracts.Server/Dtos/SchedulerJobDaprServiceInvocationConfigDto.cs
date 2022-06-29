// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class SchedulerJobDaprServiceInvocationConfigDto
{
    public string MethodName { get; set; } = string.Empty;

    public HttpMethods HttpMethod { get; set; }

    public string Data { get; set; } = string.Empty;

    public string DaprServiceIdentity { get; set; } = string.Empty;
}

