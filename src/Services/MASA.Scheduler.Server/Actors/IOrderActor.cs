namespace MASA.Scheduler.Service.Actors
{
    public interface IOrderActor : IActor
    {
        Task<List<Order>> GetListAsync();
    }
}