// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Utils;

public static class HttpUtils
{
    public static HttpMethod ConvertHttpMethod(Contracts.Server.Infrastructure.Enums.HttpMethods methods)
    {
        switch (methods)
        {
            case Contracts.Server.Infrastructure.Enums.HttpMethods.GET:
                return HttpMethod.Get;
            case Contracts.Server.Infrastructure.Enums.HttpMethods.POST:
                return HttpMethod.Post;
            case Contracts.Server.Infrastructure.Enums.HttpMethods.HEAD:
                return HttpMethod.Head;
            case Contracts.Server.Infrastructure.Enums.HttpMethods.PUT:
                return HttpMethod.Put;
            case Contracts.Server.Infrastructure.Enums.HttpMethods.DELETE:
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

    public static Uri GetRequestUrl(string requestUrl, List<KeyValuePair<string, string>> httpParameters)
    {
        var builder = new UriBuilder(requestUrl);

        builder.Query = string.Join("&", httpParameters.Select(p => $"{p.Key}={p.Value}"));

        return builder.Uri;
    }

    public static HttpContent? ConvertHttpContent(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        var contentType = "text/plain";

        if ((content.StartsWith("{") && content.EndsWith("}")) || content.StartsWith("[") && content.EndsWith("]"))
        {
            contentType = "application/json";
        }
        else if (content.StartsWith("<") && content.EndsWith(">"))
        {
            contentType = "application/xml";
        }

        return new StringContent(content, Encoding.UTF8, contentType);
    }
}

