// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Common;

public static class DaprEndpointResolver
{
    public static string Resolve(IConfiguration configuration, string endpointKey, string portKey, int defaultPort)
    {
        var endpoint = configuration.GetValue<string>(endpointKey);
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            var port = configuration.GetValue<int?>(portKey) ?? defaultPort;
            return $"http://127.0.0.1:{port}";
        }

        endpoint = endpoint.Trim();
        if (!endpoint.Contains("://", StringComparison.Ordinal))
        {
            endpoint = $"http://{endpoint}";
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            return endpoint;
        }

        if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new UriBuilder(uri)
            {
                Host = "127.0.0.1"
            };
            return builder.Uri.ToString().TrimEnd('/');
        }

        return endpoint.TrimEnd('/');
    }
}
