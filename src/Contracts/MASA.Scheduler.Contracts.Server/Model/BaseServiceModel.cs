// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Model;

public class BaseServiceModel
{
    public string ServiceId { get; set; } = string.Empty;

    public string? HttpServiceUrl { get; set; }

    public string? HttpsServiceUrl { get; set; }

    public DateTimeOffset LastResponseTime { get; set; }

    public ServiceStatus Status { get; set; }

    public int NotResponseCount { get; set; }

    public string HeartbeatApi { get; set; } = string.Empty;

    public string GetServiceUrl()
    {
        if (!string.IsNullOrWhiteSpace(HttpsServiceUrl))
        {
            return HttpsServiceUrl;
        }
        else if (!string.IsNullOrWhiteSpace(HttpServiceUrl))
        {
            return HttpServiceUrl;
        }

        return string.Empty;
    }

    public string GetHeartbeatApiUrl()
    {
        var serviceUrl = GetServiceUrl();

        return serviceUrl + HeartbeatApi;
    }
}
