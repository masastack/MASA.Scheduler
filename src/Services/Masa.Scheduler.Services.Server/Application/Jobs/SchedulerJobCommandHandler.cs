// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs;

public class SchedulerJobCommandHandler
{
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly IMapper _mapper;

    public SchedulerJobCommandHandler(ISchedulerJobRepository schedulerJobRepository, IMapper mapper)
    {
        _schedulerJobRepository = schedulerJobRepository;
        _mapper = mapper;
    }

    [EventHandler]
    public async Task AddHandleAsync(AddSchedulerJobCommand command)
    {
        var job = _mapper.Map<SchedulerJob>(command.Request.Data);

        await _schedulerJobRepository.AddAsync(job);
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
    }
}