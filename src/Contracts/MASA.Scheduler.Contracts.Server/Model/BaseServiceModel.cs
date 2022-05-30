// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Model;

public class BaseServiceModel
{
    public Guid ServiceId { get; set; }

    public string HttpHost { get; set; } = string.Empty;

    public string HttpsHost { get; set; } = string.Empty;

    public int HttpPort { get; set; }

    public int HttpsPort { get; set; }

    public DateTimeOffset LastResponseTime { get; set; }

    public ServiceStatuses Status { get; set; }

    public int NotResponseCount { get; set; }

    public string HeartbeatApi { get; set; } = string.Empty;

    public HttpClient CallerClient { get; set; } = default!;

    public string GetServiceUrl(bool containsScheme = true)
    {
        if (!string.IsNullOrWhiteSpace(HttpsHost))
        {
            var scheme = containsScheme ? "https://" : "";

            if(HttpsPort == 443)
            {
                return $"{scheme}{HttpsHost}";
            }

            return $"{scheme}{HttpsHost}:{HttpsPort}";
        }
        else if (!string.IsNullOrWhiteSpace(HttpHost))
        {
            var scheme = containsScheme ? "http://" : "";

            if(HttpPort == 80)
            {
                return $"{scheme}{HttpHost}";
            }

            return $"{scheme}{HttpHost}:{HttpPort}";
        }

        return string.Empty;
    }
}
