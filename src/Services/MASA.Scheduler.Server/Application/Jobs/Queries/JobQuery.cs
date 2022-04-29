namespace MASA.Scheduler.Service.Application.Jobs.Queries
{
    public record JobQuery : DomainQuery<List<Job>>
    {
        public override List<Job> Result { get; set; } = new();
    }
}