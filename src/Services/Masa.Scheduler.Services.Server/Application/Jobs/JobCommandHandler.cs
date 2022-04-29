namespace MASA.Scheduler.Service.Application.Jobs
{
    public class JobCommandHandler
    {
        private readonly JobDomainService _domainService;

        public JobCommandHandler(JobDomainService domainService)
        {
            _domainService = domainService;
        }

        [EventHandler(Order = 1)]
        public async Task CreateHandleAsync(JobCreateCommand command)
        {
            await _domainService.CreateJobAsync();
            //you work
            await Task.CompletedTask;
        }
    }

    public class JobStockHandler : CommandHandler<JobCreateCommand>
    {
        public override Task CancelAsync(JobCreateCommand comman)
        {
            //cancel todo callback 
            return Task.CompletedTask;
        }

        [EventHandler(FailureLevels = FailureLevels.ThrowAndCancel)]
        public override Task HandleAsync(JobCreateCommand comman)
        {
            //todo decrease stock
            return Task.CompletedTask;
        }

        [EventHandler(0, FailureLevels.Ignore, IsCancel = true)]
        public Task AddCancelLogs(JobCreateCommand query)
        {
            //todo increase stock
            return Task.CompletedTask;
        }
    }
}