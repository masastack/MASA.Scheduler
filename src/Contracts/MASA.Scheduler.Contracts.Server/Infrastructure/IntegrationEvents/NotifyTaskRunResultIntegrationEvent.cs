// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;

public record NotifyTaskRunResultIntegrationEvent : IntegrationEvent
{
    public override string Topic { get; set; } = nameof(NotifyTaskRunResultIntegrationEvent);

    public Guid TaskId { get; set; }

    public bool IsSuccess { get; set; }

    public bool IsCancel { get; set; }
}
