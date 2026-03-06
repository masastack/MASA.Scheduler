// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public class QuartzSchedulerBackend : ISchedulerBackend
{
    private readonly QuartzUtils _quartzUtils;

    public QuartzSchedulerBackend(QuartzUtils quartzUtils)
    {
        _quartzUtils = quartzUtils;
    }

    public Task StartAsync()
    {
        return _quartzUtils.StartQuartzScheduler();
    }

    public Task RegisterCronJob(string environment, Guid jobId, string cron)
    {
        return _quartzUtils.RegisterCronJob<StartSchedulerJobQuartzJob>(environment, jobId, cron);
    }

    public Task RemoveCronJob(Guid jobId)
    {
        return _quartzUtils.RemoveCronJob(jobId);
    }

    public Task AddDelayTask(string environment, Guid taskId, Guid jobId, TimeSpan delay)
    {
        return _quartzUtils.AddDelayTask<StartSchedulerTaskQuartzJob>(environment, taskId, jobId, delay);
    }

    public Task RemoveDelayTask(Guid taskId, Guid jobId)
    {
        return _quartzUtils.RemoveDelayTask(taskId, jobId);
    }

    public Task<List<DateTimeOffset>> GetCronExecuteTimeByTimeRange(string cron, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        return _quartzUtils.GetCronExcuteTimeByTimeRange(cron, startTime, endTime);
    }
}
