namespace MASA.Scheduler.Service.Infrastructure
{
    public class ShopDbContext : IntegrationEventLogContext
    {
        public DbSet<Job> Jobs { get; set; } = default!;

        public ShopDbContext(MasaDbContextOptions<ShopDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreatingExecuting(ModelBuilder builder)
        {
            base.OnModelCreatingExecuting(builder);
        }
    }
}