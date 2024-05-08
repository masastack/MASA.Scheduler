// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Utils;

public static class HttpUtils
{
    public static ActivitySource ActivitySource { get; private set; } = new ActivitySource("Masa.Scheduler.Background");

    public static HttpMethod ConvertHttpMethod(HttpMethods methods)
    {
        switch (methods)
        {
            case HttpMethods.GET:
                return HttpMethod.Get;
            case HttpMethods.POST:
                return HttpMethod.Post;
            case HttpMethods.HEAD:
                return HttpMethod.Head;
            case HttpMethods.PUT:
                return HttpMethod.Put;
            case HttpMethods.DELETE:
                return HttpMethod.Delete;
            default:
                throw new UserFriendlyException($"Cannot convert method: {methods}");
        }
    }

    public static void AddHttpHeader(HttpClient client, List<KeyValuePair<string, string>> httpHeaders)
    {
        foreach (var header in httpHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    public static Activity? SetTraceParent(string? traceId, string? spanId)
    {
        return ActivitySource.StartActivity("Background Task", ActivityKind.Consumer, $"00-{traceId}-{spanId}-01");
    }

    public static Uri GetRequestUrl(string requestUrl, List<KeyValuePair<string, string>> httpParameters)
    {
        var builder = new UriBuilder(requestUrl)
        {
            Query = string.Join("&", httpParameters.Select(p => $"{p.Key}={p.Value}"))
        };

        return builder.Uri;
    }

    public static HttpContent? ConvertHttpContent(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        var contentType = "text/plain";

        if ((content.StartsWith('{') && content.EndsWith('}')) || content.StartsWith('[') && content.EndsWith(']'))
        {
            contentType = "application/json";
        }
        else if (content.StartsWith('<') && content.EndsWith('>'))
        {
            contentType = "application/xml";
        }

        return new StringContent(content, Encoding.UTF8, contentType);
    }
}

