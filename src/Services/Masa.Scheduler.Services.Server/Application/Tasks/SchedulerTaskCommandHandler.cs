// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Tasks;

public class SchedulerTaskCommandHandler
{
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IMapper _mapper;
    private readonly SchedulerTaskDomainService _schedulerTaskDomainService;

    public SchedulerTaskCommandHandler(ISchedulerTaskRepository schedulerTaskRepository, IMapper mapper, SchedulerTaskDomainService schedulerTaskDomainService)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _mapper = mapper;
        _schedulerTaskDomainService = schedulerTaskDomainService;
    }

    [EventHandler]
    public async Task AddHandleAsync(AddSchedulerTaskCommand command)
    {
        var task = new SchedulerTask(command.Request.JobId, command.Request.Origin, command.Request.RunUserId);

        await _schedulerTaskRepository.AddAsync(task);
    }

    [EventHandler]
    public async Task StartHandleAsync(StartSchedulerTaskCommand command)
    {
        await _schedulerTaskDomainService.StartTaskAsync(command.Request);
    }

    [EventHandler]
    public async Task StopHandleAsync(StopSchedulerTaskCommand command)
    {
        await _schedulerTaskDomainService.StopTaskAsync(command.Request);
    }

    [EventHandler]
    public async Task RemoveHandleAsync(RemoveSchedulerTaskCommand command)
    {
        await _schedulerTaskDomainService.RemoveTaskAsync(command.Request);
    }

    [EventHandler]
    public async Task NotifyTaskRunResultHandleAsync(NotifySchedulerTaskRunResultCommand command)
    {
        await _schedulerTaskDomainService.NotifyTaskRunResultAsync(command.Request);
    }
}
