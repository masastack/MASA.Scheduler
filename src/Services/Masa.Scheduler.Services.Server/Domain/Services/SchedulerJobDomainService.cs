// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Services;

public class SchedulerJobDomainService : DomainService
{
    private readonly ILogger<SchedulerJob> _logger;
    private readonly ISchedulerJobRepository _schedulerJobRepository;

    public SchedulerJobDomainService(IDomainEventBus eventBus, ILogger<SchedulerJob> logger, ISchedulerJobRepository schedulerJobRepository) : base(eventBus)
    {
        _logger = logger;
        _schedulerJobRepository = schedulerJobRepository;
    }

    public async Task StartJobAsync(StartSchedulerJobRequest request)
    {
        await EventBus.PublishAsync(new StartJobDomainEvent(request));
    }

    public async Task RegisterCronJobAsync(RegisterCronJobRequest request)
    {

    }
}
