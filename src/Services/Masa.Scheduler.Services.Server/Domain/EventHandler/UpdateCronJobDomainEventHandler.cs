// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.EventHandler;

public class UpdateCronJobDomainEventHandler
{
    private readonly QuartzUtils _quartzUtils;
    private readonly IRepository<SchedulerTask> _schedulerTaskRepository;
    private readonly IRepository<SchedulerJob> _schedulerJobRepository;

    public UpdateCronJobDomainEventHandler(QuartzUtils quartzUtils, IRepository<SchedulerTask> schedulerTaskRepository, IRepository<SchedulerJob> schedulerJobRepository)
    {
        _quartzUtils = quartzUtils;
        _schedulerTaskRepository = schedulerTaskRepository;
        _schedulerJobRepository = schedulerJobRepository;
    }

    [EventHandler]
    public async Task UpdateCronJobAsync(UpdateCronJobDomainEvent @event)
    {
        if(@event.Request.ScheduleType == ScheduleTypes.Cron && !string.IsNullOrWhiteSpace(@event.Request.CronExpression) && @event.Request.Enabled)
        {
            await _quartzUtils.RegisterCronJob<StartSchedulerJobQuartzJob>(@event.Request.JobId, @event.Request.CronExpression);
        }
        else
        {
            var waitingTasks = await _schedulerTaskRepository.GetListAsync(p => p.JobId == @event.Request.JobId && (p.TaskStatus == TaskRunStatus.WaitToRetry || p.TaskStatus == TaskRunStatus.WaitToRun));

            foreach (var task in waitingTasks)
            {
                task.TaskEnd(TaskRunStatus.Failure, "Job is disabled");
                await _schedulerTaskRepository.UpdateAsync(task);
            }

            var job = await _schedulerJobRepository.FindAsync(p => p.Id == @event.Request.JobId);

            if(job != null)
            {
                job.UpdateLastRunDetail(TaskRunStatus.Failure);
                await _schedulerJobRepository.UpdateAsync(job);
            }

            await _quartzUtils.RemoveCronJob(@event.Request.JobId);
        }
    }
}
