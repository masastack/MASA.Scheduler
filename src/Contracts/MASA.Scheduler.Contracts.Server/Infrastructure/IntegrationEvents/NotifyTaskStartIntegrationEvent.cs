// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;

public record NotifyTaskStartIntegrationEvent : IntegrationEvent
{
    public override string Topic { get; set; } = nameof(NotifyTaskStartIntegrationEvent);

    public Guid TaskId { get; set; }
}

