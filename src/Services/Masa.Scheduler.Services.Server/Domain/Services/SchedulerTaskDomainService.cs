// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Services;

public class SchedulerTaskDomainService : DomainService
{
    private readonly ILogger<SchedulerTask> _logger;
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    public SchedulerTaskDomainService(IDomainEventBus eventBus, ILogger<SchedulerTask> logger, ISchedulerTaskRepository schedulerTaskRepository) : base(eventBus)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _logger = logger;
    }

    public async Task StartTaskAsync(StartSchedulerTaskRequest request)
    {
        await EventBus.PublishAsync(new StartTaskDomainEvent(request));
    }

    public async Task StopTaskAsync(StopSchedulerTaskRequest request)
    {
        await EventBus.PublishAsync(new StopTaskDomainEvent(request));
    }

    public async Task RemoveTaskAsync(RemoveSchedulerTaskRequest request)
    {
        var task = await _schedulerTaskRepository.FindAsync(t => t.Id == request.TaskId);

        if(task is null)
        {
            throw new UserFriendlyException($"SchedulerTask not found, taskId: {request.TaskId}");
        }

        if(task.TaskStatus == TaskRunStatuses.Running)
        {
            throw new UserFriendlyException($"Task is running, cannot delete, taskId: {request.TaskId}");
        }

        await _schedulerTaskRepository.RemoveAsync(task);

        _logger.LogInformation($"User manual delete SchedulerTask, TaskId: {task.Id}, OperatorId: {request.OperatorId}");
    }
}
