namespace MASA.Scheduler.Service.Domain.Aggregates.Jobs
{
    public class Job : AggregateRoot<int>
    {
        public Job()
        {
        }

        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

        public string OrderNumber { get; set; } = default!;

        public string Address { get; set; } = default!;
    }
}