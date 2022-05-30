// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class NotifyTaskRunResultDomainEventHandler
{
    private readonly IRepository<SchedulerTask> _schedulerTaskRepository;
    private readonly SchedulerDbContext _dbContext;
    private readonly IRepository<SchedulerJob> _schedulerJobRepository;

    public NotifyTaskRunResultDomainEventHandler(IRepository<SchedulerTask> schedulerTaskRepository, SchedulerDbContext dbContext, IRepository<SchedulerJob> schedulerJobRepository)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _dbContext = dbContext;
        _schedulerJobRepository = schedulerJobRepository;
    }

    [EventHandler]
    public async Task NotifyTaskRunResultAsync(NotifyTaskRunResultDomainEvent @event)
    {
        var task = await _dbContext.Tasks.Include(t => t.Job).FirstOrDefaultAsync(t => t.Id == @event.Request.TaskId);

        if (task == null)
        {
            throw new UserFriendlyException($"cannot find task, task Id: {@event.Request.TaskId}");
        }

        TaskRunStatuses status;
        string message;

        switch (@event.Request.Status)
        {
            case TaskRunResultStatuses.Success:
                status = TaskRunStatuses.Success;
                message = "Task run success";
                break;
            case TaskRunResultStatuses.Stop:
                status = TaskRunStatuses.Failure;
                message = "Task run stop";
                break;
            default:
                status = TaskRunStatuses.Failure;
                message = "Task run failure";
                break;
        }

        if (task.TaskStatus == TaskRunStatuses.Timeout && status == TaskRunStatuses.Success)
        {
            status = TaskRunStatuses.TimeoutSuccess;

            message = "Task run success, but timeout";
        }

        task.TaskEnd(status, message);

        task.Job.UpdateLastRunDetail(status);

        await _schedulerJobRepository.UpdateAsync(task.Job);

        await _schedulerTaskRepository.UpdateAsync(task);
    }
}
