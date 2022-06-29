// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class RemoveSchedulerJobDomainEventHandler
{
    private readonly QuartzUtils _quartzUtils;
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly IDomainEventBus _eventBus;
    private readonly SchedulerServerManager _schedulerServerManager;

    public RemoveSchedulerJobDomainEventHandler(QuartzUtils quartzUtils, ISchedulerJobRepository schedulerJobRepository, ISchedulerTaskRepository schedulerTaskRepository, IDomainEventBus eventBus, SchedulerServerManager schedulerServerManager)
    {
        _quartzUtils = quartzUtils;
        _schedulerJobRepository = schedulerJobRepository;
        _schedulerTaskRepository = schedulerTaskRepository;
        _eventBus = eventBus;
        _schedulerServerManager = schedulerServerManager;
    }

    [EventHandler]
    public async Task RemoveSchedulerJobAsync(RemoveSchedulerJobDomainEvent @event)
    {
        var job = await _schedulerJobRepository.FindAsync(@event.Request.JobId);

        if (job is null)
        {
            throw new UserFriendlyException($"Job id {@event.Request.JobId}, not found");
        }

        await _schedulerJobRepository.RemoveAsync(job);

        var filterStatus = new List<TaskRunStatus>() { TaskRunStatus.Running, TaskRunStatus.WaitToRun, TaskRunStatus.WaitToRetry };

        var taskList = await _schedulerTaskRepository.GetListAsync(p => p.JobId == @event.Request.JobId && filterStatus.Contains(p.TaskStatus));

        foreach (var task in taskList)
        {
            await _schedulerServerManager.StopTask(task.Id, task.WorkerHost);
            task.TaskEnd(TaskRunStatus.Failure, "stop by job remove");
            await _schedulerTaskRepository.UpdateAsync(task);
        }

        await _quartzUtils.RemoveCronJob(@event.Request.JobId);
    }
}
