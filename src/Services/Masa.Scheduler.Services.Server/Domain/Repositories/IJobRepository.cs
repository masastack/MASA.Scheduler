namespace MASA.Scheduler.Service.Domain.Repositories
{

    public interface IJobRepository : IRepository<Job>
    {
        Task<List<Job>> GetListAsync();
    }
}