namespace MASA.Scheduler.Service.Services
{
    public class JobService : ServiceBase
    {
        public JobService(IServiceCollection services) : base(services)
        {
            App.MapGet("/job/list", QueryList).Produces<List<Job>>()
                .WithName("GetJobs")
                .RequireAuthorization();
            App.MapPost("/createJob", CreateJob);
        }


        public async Task<IResult> QueryList(JobDomainService jobDomainService)
        {
            var jobs = await jobDomainService.QueryListAsync();
            return Results.Ok(jobs);
        }

        public async Task<IResult> CreateJob(IEventBus eventBus)
        {
            var comman = new JobCreateCommand();
            await eventBus.PublishAsync(comman);
            return Results.Ok();
        }
    }
}