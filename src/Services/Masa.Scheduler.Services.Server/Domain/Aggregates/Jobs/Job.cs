namespace MASA.Scheduler.Service.Domain.Aggregates.Jobs
{
    public class Job : AggregateRoot<int>
    {
        public Job(string orderNumber, string address)
        {
            OrderNumber = orderNumber;
            Address = address;
        }

        public DateTimeOffset CreationTime { get; private set; } = DateTimeOffset.Now;

        public string OrderNumber { get; private set; } = default!;

        public string Address { get; private set; } = default!;
    }
}