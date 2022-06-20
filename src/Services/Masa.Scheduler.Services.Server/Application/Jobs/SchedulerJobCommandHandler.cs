// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobCommandHandler
{
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly IMapper _mapper;
    private readonly SchedulerJobDomainService _schedulerJobDomainService;
    private readonly QuartzUtils _quartzUtils;

    public SchedulerJobCommandHandler(ISchedulerJobRepository schedulerJobRepository, IMapper mapper, SchedulerJobDomainService schedulerJobDomainService, QuartzUtils quartzUtils)
    {
        _schedulerJobRepository = schedulerJobRepository;
        _mapper = mapper;
        _schedulerJobDomainService = schedulerJobDomainService;
        _quartzUtils = quartzUtils;
    }

    [EventHandler]
    public async Task AddHandleAsync(AddSchedulerJobCommand command)
    {
        var job = _mapper.Map<SchedulerJob>(command.Request.Data);

        await _schedulerJobRepository.AddAsync(job);

        var request = new RegisterCronJobRequest()
        {
            Data = command.Request.Data
        };

        await _schedulerJobDomainService.RegisterCronJobAsync(request);
    }

    [EventHandler]
    public async Task UpdateHandleAsync(UpdateSchedulerJobCommand command)
    {
        var jobDto = command.Request.Data;

        var job = await _schedulerJobRepository.FindAsync(job => job.Id == jobDto.Id);

        if(job is null)
        {
            throw new UserFriendlyException($"The current job does not exist");
        }

        job.UpdateJob(jobDto);

        await _schedulerJobRepository.UpdateAsync(job);

        var request = new RegisterCronJobRequest()
        {
            Data = jobDto
        };

        await _schedulerJobDomainService.RegisterCronJobAsync(request);
    }

    [EventHandler]
    public async Task RemoveHandleAsync(RemoveSchedulerJobCommand command)
    {
        var job = await _schedulerJobRepository.FindAsync(command.JobId);

        if(job is null)
        {
            throw new UserFriendlyException($"Job id {command.JobId}, not found");
        }

        await _schedulerJobRepository.RemoveAsync(job);

        await _quartzUtils.RemoveCronJob(job.Id);
    }

    [EventHandler]
    public async Task ChangeEnabledStatusHandleAsync(ChangeEnableStatusSchedulerJobCommand command)
    {
        var job = await _schedulerJobRepository.FindAsync(command.Request.Id);

        if (job is null)
        {
            throw new UserFriendlyException($"Job id {command.Request.Id}, not found");
        }

        job.ChangeEnableStatus(command.Request.Enabled);

        await _schedulerJobRepository.UpdateAsync(job);
    }

    [EventHandler]
    public async Task StartJobHandleAsync(StartSchedulerJobCommand command)
    {
        await _schedulerJobDomainService.StartJobAsync(command.Request);
    }
}
