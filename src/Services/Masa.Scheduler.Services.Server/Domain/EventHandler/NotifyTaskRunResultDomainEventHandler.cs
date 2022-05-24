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
            return;
        }

        var status = @event.Request.IsSuccess ? TaskRunStatuses.Success : TaskRunStatuses.Failure;
        var message = @event.Request.IsSuccess ? "Task run success" : (@event.Request.IsCancel ? "Task run failure" : "Task run stop");

        if (task.TaskStatus == TaskRunStatuses.Timeout && @event.Request.IsSuccess)
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
