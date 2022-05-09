namespace MASA.Scheduler.Service.Application.Jobs;

public class JobQueryHandler
{
    readonly IJobRepository _jobRepository;
    public JobQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    [EventHandler]
    public async Task JobListHandleAsync(JobQuery query)
    {
        var actorId = new ActorId(Guid.NewGuid().ToString());
        var actor = ActorProxy.Create<IJobActor>(actorId, nameof(JobActor));
        query.Result = await actor.GetListAsync();
    }
}
