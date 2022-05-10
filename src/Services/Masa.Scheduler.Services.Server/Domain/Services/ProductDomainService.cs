// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Services;

public class ProductDomainService : DomainService
{
    public ProductDomainService(IDomainEventBus eventBus) : base(eventBus)
    {
    }

    [EventHandler(Order = 1)]
    public void DeductInvenroyCompletedAsync(SchedulerJobCreatedDomainEvent @event)
    {
    }

    [EventHandler(Order = 0, FailureLevels = FailureLevels.ThrowAndCancel)]
    public Task DeductInvenroyAsync(SchedulerJobCreatedDomainEvent @event)
    {
        throw new NotImplementedException();
    }

    [EventHandler(1, FailureLevels.Ignore, IsCancel = true)]
    public Task CancelDeductInvenroyAsync(SchedulerJobCreatedDomainEvent @event)
    {
        throw new NotImplementedException();
    }

}
