using MASA.Scheduler.Contracts.Server.Model;

namespace MASA.Scheduler.Caller.Callers
{
    public class JobCaller : HttpClientCallerBase
    {
        protected override string BaseAddress { get; set; } = "http://localhost:16002";

        public JobCaller(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Name = nameof(JobCaller);
        }

        public async Task<List<Job>> GetListAsync()
        {
            var result = await CallerProvider.GetAsync<List<Job>>($"job/list");
            return result ?? new List<Job>();
        }
    }
}