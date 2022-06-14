// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.IntegrationEvents;

public abstract record OnlineIntegrationEvent : BaseIntegrationEvent
{
    public BaseServiceModel OnlineService { get; set; } = default!;

    public bool IsPong { get; set; }
}
