// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobHttpConfig
{
    public HttpMethods HttpMethod { get; private set; }

    public string RequestUrl { get; private set; } = string.Empty;

    public Dictionary<string, string> HttpParameter { get; private set; } = new();

    public Dictionary<string, string> HttpHeaders { get; private set; } = new();

    public string HttpBody { get; private set; } = string.Empty;

    public HttpVerifyTypes HttpVerifyType { get; private set; }

    public string VerifyContent { get; private set; } = string.Empty;

    public void SetConfig(HttpMethods httpMethod, string requestUrl, Dictionary<string, string> httpParameter, Dictionary<string, string> httpHeader, string httpBody, HttpVerifyTypes httpVerifyType, string verityContent)
    {
        HttpMethod = httpMethod;
        RequestUrl = requestUrl;
        HttpParameter = httpParameter;
        HttpHeaders = httpHeader;
        HttpBody = httpBody;
        HttpVerifyType = httpVerifyType;
        VerifyContent = verityContent;
    }
}
