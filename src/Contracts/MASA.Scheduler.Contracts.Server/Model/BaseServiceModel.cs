// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Model;

public class BaseServiceModel
{
    public string ServiceId { get; set; } = string.Empty;

    public string HttpHost { get; set; } = string.Empty;

    public string HttpsHost { get; set; } = string.Empty;

    public int HttpPort { get; set; }

    public int HttpsPort { get; set; }

    public DateTimeOffset LastResponseTime { get; set; }

    public ServiceStatus Status { get; set; }

    public int NotResponseCount { get; set; }

    public string HeartbeatApi { get; set; } = string.Empty;

    public string GetServiceUrl()
    {
        if (!string.IsNullOrWhiteSpace(HttpsHost))
        {
            var scheme = "https://";

            if(HttpsPort == 443)
            {
                return $"{scheme}{HttpsHost}";
            }

            return $"{scheme}{HttpsHost}:{HttpsPort}";
        }
        else if (!string.IsNullOrWhiteSpace(HttpHost))
        {
            var scheme = "http://";

            if(HttpPort == 80)
            {
                return $"{scheme}{HttpHost}";
            }

            return $"{scheme}{HttpHost}:{HttpPort}";
        }

        return string.Empty;
    }
}
