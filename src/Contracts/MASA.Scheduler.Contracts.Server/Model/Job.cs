namespace MASA.Scheduler.Contracts.Server.Model
{
    public class Job
    {
        public DateTime CreationTime { get; set; }

        public int Id { get; set; }

        public string OrderNumber { get; set; } = "";

        public string Address { get; set; } = default!;
    }
}