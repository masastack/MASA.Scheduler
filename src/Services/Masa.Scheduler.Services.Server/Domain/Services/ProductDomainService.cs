namespace MASA.Scheduler.Service.Domain.Services
{
    public class ProductDomainService : DomainService
    {

        public ProductDomainService(IDomainEventBus eventBus) : base(eventBus)
        {
        }

        [EventHandler(Order = 1)]
        public void DeductInvenroyCompletedAsync(JobCreatedDomainEvent @event)
        {

        }

        [EventHandler(Order = 0, FailureLevels = FailureLevels.ThrowAndCancel)]
        public Task DeductInvenroyAsync(JobCreatedDomainEvent @event)
        {
            throw new NotImplementedException();
        }

        [EventHandler(1, FailureLevels.Ignore, IsCancel = true)]
        public Task CancelDeductInvenroyAsync(JobCreatedDomainEvent @event)
        {
            throw new NotImplementedException();
        }

    }
}
