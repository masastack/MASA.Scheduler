// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Services
{
    public class ProductDomainService : DomainService
    // you can alse implement an interface like below
    //, ISagaEventHandler<OrderCreatedDomainEvent>
    {

        public ProductDomainService(IDomainEventBus eventBus) : base(eventBus)
        {
        }

        [EventHandler(Order = 1)]
        public void DeductInvenroyCompletedAsync(JobCreatedDomainEvent @event)
        {
            //todo after decrease stock,like Pub Event to other micro service
        }

        [EventHandler(Order = 0, FailureLevels = FailureLevels.ThrowAndCancel)]
        public Task DeductInvenroyAsync(JobCreatedDomainEvent @event)
        {
            //todo decrease stock
            throw new NotImplementedException();
        }

        [EventHandler(1, FailureLevels.Ignore, IsCancel = true)]
        public Task CancelDeductInvenroyAsync(JobCreatedDomainEvent @event)
        {
            //throw exception,todo increase stock
            throw new NotImplementedException();
        }

    }
}
