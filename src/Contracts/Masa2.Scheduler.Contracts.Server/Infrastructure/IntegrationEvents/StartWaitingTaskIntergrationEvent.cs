// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;

public record StartWaitingTaskIntergrationEvent : IntegrationEvent
{
    public override string Topic { get; set; } = nameof(StartWaitingTaskIntergrationEvent);

    public Guid TaskId { get; set; }

    public Guid OperatorId { get; set; }
}

