namespace MASA.Scheduler.Service.Infrastructure;

public class SchedulerDbContext : IsolationDbContext
{
    public DbSet<Job> Jobs { get; set; } = default!;

    public SchedulerDbContext(MasaDbContextOptions<SchedulerDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreatingExecuting(ModelBuilder builder)
    {
        base.OnModelCreatingExecuting(builder);
    }
}
