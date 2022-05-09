namespace MASA.Scheduler.Service.Infrastructure.Repositories;

public class JobRepository : Repository<SchedulerDbContext, Job>, IJobRepository
{
    public JobRepository(SchedulerDbContext context, IUnitOfWork unitOfWork)
        : base(context, unitOfWork)
    {
    }
    public async Task<List<Job>> GetListAsync()
    {
        var data = Enumerable.Range(1, 5).Select(index =>
              new Job(DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), $"Address {index}")).ToList();
        return await Task.FromResult(data);
    }
}
