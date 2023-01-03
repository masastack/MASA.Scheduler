// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;

public record StartTaskIntegrationEvent : BaseIntegrationEvent
{
    public override string Topic { get; set; } = nameof(StartTaskIntegrationEvent);

    public SchedulerJobDto Job { get; set; } = default!;

    public Guid TaskId { get; set; }

    public DateTimeOffset ExcuteTime { get; set; }
}
