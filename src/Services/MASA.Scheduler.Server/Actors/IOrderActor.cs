namespace MASA.Scheduler.Service.Actors
{
    public interface IJobActor : IActor
    {
        Task<List<Job>> GetListAsync();
    }
}