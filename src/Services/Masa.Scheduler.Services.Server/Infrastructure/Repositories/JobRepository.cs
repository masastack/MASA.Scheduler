namespace MASA.Scheduler.Service.Infrastructure.Repositories
{
    public class JobRepository : Repository<SchedulerDbContext, Job>, IJobRepository
    {
        public JobRepository(SchedulerDbContext context, IUnitOfWork unitOfWork)
            : base(context, unitOfWork)
        {
        }
        public async Task<List<Job>> GetListAsync()
        {
            var data = Enumerable.Range(1, 5).Select(index =>
                  new Job
                  {
                      CreationTime = DateTimeOffset.Now,
                      OrderNumber = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                      Address = $"Address {index}"
                  }).ToList();
            return await Task.FromResult(data);
        }
    }
}