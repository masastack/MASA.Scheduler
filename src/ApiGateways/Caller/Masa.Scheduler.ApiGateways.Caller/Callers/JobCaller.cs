namespace MASA.Scheduler.Caller.Callers;

public class JobCaller : HttpClientCallerBase
{
    private readonly IConfiguration _configuration;

    protected override string BaseAddress { get; set; }

    public JobCaller(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
    {
        Name = nameof(JobCaller);
        _configuration = configuration;
        BaseAddress = configuration["SchedulerServerBaseAddress"];
    }

    public async Task<List<Job>> GetListAsync()
    {
        var result = await CallerProvider.GetAsync<List<Job>>($"job/list");
        return result ?? new List<Job>();
    }
}
