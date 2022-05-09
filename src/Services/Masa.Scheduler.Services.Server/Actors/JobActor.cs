namespace MASA.Scheduler.Service.Actors;

public class JobActor : Actor, IJobActor
{
    readonly IJobRepository _jobRepository;

    public JobActor(ActorHost host, IJobRepository jobRepository) : base(host)
    {
        _jobRepository = jobRepository;
    }

    public async Task<List<Job>> GetListAsync()
    {
        var data = await _jobRepository.GetListAsync();
        return data;
    }
}
