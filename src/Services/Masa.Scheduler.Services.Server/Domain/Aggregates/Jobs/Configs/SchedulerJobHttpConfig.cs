// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobHttpConfig
{
    public Guid JobId { get; private set; }

    public HttpMethods HttpMethod { get; private set; }

    public string RequestUrl { get; private set; } = string.Empty;

    public string HttpParameter { get; private set; } = string.Empty;

    public string HttpHeaders { get; private set; }

    public string HttpBody { get; set; } = string.Empty;

    public HttpVerifyTypes HttpVerifyType { get; private set; }

    public string VerifyContent { get; private set; } = string.Empty;

    public SchedulerJobHttpConfig(Guid jobId, HttpMethods httpMethod, string requestUrl, string httpParameter, string httpHeader, string httpBody, HttpVerifyTypes httpVerifyType, string verityContent)
    {
        JobId = jobId;
        HttpMethod = httpMethod;
        RequestUrl = requestUrl;
        HttpParameter = httpParameter;
        HttpHeaders = httpHeader;
        HttpBody = httpBody;
        HttpVerifyType = httpVerifyType;
        VerifyContent = verityContent;
    }
}
