// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;

public abstract record OnlineIntegrationEvent : BaseIntegrationEvent
{
    public string HttpHost { get; set; } = string.Empty;

    public string HttpsHost { get; set; } = string.Empty;

    public int HttpPort { get; set; }

    public int HttpsPort { get; set; }

    public bool IsResponse { get; set; }

    public string HeartbeatApi { get; set; } = string.Empty;
}
