// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class SchedulerJobHttpConfigDto
{
    public HttpMethods HttpMethod { get; set; }

    public string RequestUrl { get; set; } = string.Empty;

    public List<KeyValuePair<string, string>> HttpParameters { get; set; } = new List<KeyValuePair<string, string>>();

    public List<KeyValuePair<string, string>> HttpHeaders { get; set; } = new List<KeyValuePair<string, string>>();

    public string HttpBody { get; set; } = string.Empty;

    public HttpVerifyTypes HttpVerifyType { get; set; }

    public string VerifyContent { get; set; } = string.Empty;

    public bool IsAsync { get; set; }
}

