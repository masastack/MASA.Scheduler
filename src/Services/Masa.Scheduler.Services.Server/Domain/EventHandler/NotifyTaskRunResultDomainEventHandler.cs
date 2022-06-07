// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class NotifyTaskRunResultDomainEventHandler
{
    private readonly IRepository<SchedulerTask> _schedulerTaskRepository;
    private readonly SchedulerDbContext _dbContext;
    private readonly IRepository<SchedulerJob> _schedulerJobRepository;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public NotifyTaskRunResultDomainEventHandler(IRepository<SchedulerTask> schedulerTaskRepository, SchedulerDbContext dbContext, IRepository<SchedulerJob> schedulerJobRepository, IHubContext<NotificationsHub> hubContext)
    {
        _schedulerTaskRepository = schedulerTaskRepository;
        _dbContext = dbContext;
        _schedulerJobRepository = schedulerJobRepository;
        _hubContext = hubContext;
    }

    [EventHandler]
    public async Task NotifyTaskRunResultAsync(NotifyTaskRunResultDomainEvent @event)
    {
        var task = await _dbContext.Tasks.Include(t => t.Job).FirstOrDefaultAsync(t => t.Id == @event.Request.TaskId);

        if (task == null)
        {
            throw new UserFriendlyException($"cannot find task, task Id: {@event.Request.TaskId}");
        }

        TaskRunStatus status = @event.Request.Status;
        string message;

        switch (@event.Request.Status)
        {
            case TaskRunStatus.Success:
                message = "Task run success";
                break;
            case TaskRunStatus.Timeout:
                message = "Task run timeout";
                break;
            default:
                message = "Task run failure";
                break;
        }

        if (task.TaskStatus == TaskRunStatus.Timeout && status == TaskRunStatus.Success)
        {
            status = TaskRunStatus.TimeoutSuccess;

            message = "Task run success, but timeout";
        }

        task.TaskEnd(status, message);

        task.Job.UpdateLastRunDetail(status);

        await _schedulerJobRepository.UpdateAsync(task.Job);
        await _schedulerTaskRepository.UpdateAsync(task);

        await _schedulerJobRepository.UnitOfWork.SaveChangesAsync();
        await _schedulerJobRepository.UnitOfWork.CommitAsync();

        var groupClient = _hubContext.Clients.Groups(ConstStrings.GLOBAL_GROUP);
        await groupClient.SendAsync(SignalRMethodConsts.GET_NOTIFICATION);
    }
}
