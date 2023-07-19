// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class RemoveSchedulerJobDomainEventHandler
{
    private readonly QuartzUtils _quartzUtils;
    private readonly ISchedulerJobRepository _schedulerJobRepository;
    private readonly ISchedulerTaskRepository _schedulerTaskRepository;
    private readonly SchedulerServerManager _schedulerServerManager;
    private readonly IAlertClient _alertClient;

    public RemoveSchedulerJobDomainEventHandler(QuartzUtils quartzUtils, ISchedulerJobRepository schedulerJobRepository, ISchedulerTaskRepository schedulerTaskRepository, SchedulerServerManager schedulerServerManager, IAlertClient alertClient)
    {
        _quartzUtils = quartzUtils;
        _schedulerJobRepository = schedulerJobRepository;
        _schedulerTaskRepository = schedulerTaskRepository;
        _schedulerServerManager = schedulerServerManager;
        _alertClient = alertClient;
    }

    [EventHandler]
    public async Task RemoveSchedulerJobAsync(RemoveSchedulerJobDomainEvent @event)
    {
        var job = await _schedulerJobRepository.FindAsync(@event.Request.JobId);

        if (job is null)
        {
            throw new UserFriendlyException($"Job id {@event.Request.JobId}, not found");
        }

        job.NotifyJobStatus(JobNotifyStatus.Delete);

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

        if (job.AlarmRuleId != default)
        {
            await _alertClient.AlarmRuleService.DeleteAsync(job.AlarmRuleId);
        }
    }
}
