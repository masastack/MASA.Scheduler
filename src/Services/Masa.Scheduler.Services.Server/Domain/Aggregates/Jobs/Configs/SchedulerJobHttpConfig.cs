// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.Configs;

public class SchedulerJobHttpConfig : ValueObject
{
    [JsonInclude]
    public HttpMethods HttpMethod { get; private set; }

    [JsonInclude]
    public string RequestUrl { get; private set; } = string.Empty;

    [JsonInclude]
    public List<KeyValuePair<string, string>> HttpParameters { get; private set; } = new();

    [JsonInclude]
    public List<KeyValuePair<string, string>> HttpHeaders { get; private set; } = new();

    [JsonInclude]
    public string HttpBody { get; private set; } = string.Empty;

    [JsonInclude]
    public HttpVerifyTypes HttpVerifyType { get; private set; }

    [JsonInclude]
    public string VerifyContent { get; private set; } = string.Empty;

    [JsonInclude]
    public bool IsAsync { get; set; }

    public void SetConfig(HttpMethods httpMethod, string requestUrl, List<KeyValuePair<string, string>> httpParameters, List<KeyValuePair<string, string>> httpHeader, string httpBody, HttpVerifyTypes httpVerifyType, string verityContent, bool isAsync)
    {
        HttpMethod = httpMethod;
        RequestUrl = requestUrl;
        HttpParameters = httpParameters;
        HttpHeaders = httpHeader;
        HttpBody = httpBody;
        HttpVerifyType = httpVerifyType;
        VerifyContent = verityContent;
        IsAsync = isAsync;
    }

    protected override IEnumerable<object> GetEqualityValues()
    {
        yield return HttpMethod;
        yield return RequestUrl;
        yield return HttpParameters;
        yield return HttpHeaders;
        yield return HttpBody;
        yield return HttpVerifyType;
        yield return VerifyContent;
        yield return IsAsync;
    }
}
