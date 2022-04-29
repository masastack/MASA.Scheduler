namespace MASA.Scheduler.Service.Domain.Events
{
    public record JobQueryDomainEvent : DomainEvent
    {
        public List<Job> Jobs { get; set; } = new();
    }
}