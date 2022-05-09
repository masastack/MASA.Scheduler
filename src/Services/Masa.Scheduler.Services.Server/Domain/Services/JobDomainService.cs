namespace MASA.Scheduler.Service.Domain.Services;

public class JobDomainService : DomainService
{
    private readonly ILogger<Job> _logger;
    private readonly IJobRepository _jobRepository;

    public JobDomainService(IDomainEventBus eventBus, ILogger<Job> logger, IJobRepository jobRepository) : base(eventBus)
    {
        _logger = logger;
        _jobRepository = jobRepository;
    }

    public async Task CreateJobAsync()
    {
        //todo create order
        var orderEvent = new JobCreatedDomainEvent();
        await EventBus.PublishAsync(orderEvent);
    }

    public async Task<IList<Job>> QueryListAsync()
    {
        var actorId = new ActorId(Guid.NewGuid().ToString());
        var actor = ActorProxy.Create<IJobActor>(actorId, nameof(JobActor));
        return await actor.GetListAsync();
    }
}
