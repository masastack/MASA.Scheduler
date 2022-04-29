using MASA.Scheduler.Contracts.Order.Model;

namespace MASA.Scheduler.Caller.Callers
{
    public class OrderCaller : HttpClientCallerBase
    {
        protected override string BaseAddress { get; set; } = "http://localhost:6239";

        public OrderCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(OrderCaller);
        }

        public async Task<List<Order>> GetListAsync()
        {
            var result = await CallerProvider.GetAsync<List<Order>>($"order/list");
            return result ?? new List<Order>();
        }
    }
}