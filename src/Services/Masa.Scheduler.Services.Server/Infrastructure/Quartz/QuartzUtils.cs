// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Quartz;

public class QuartzUtils
{
    private readonly ISchedulerFactory _schedulerFactory;
    private IScheduler _scheduler = default!;

    public QuartzUtils(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task StartQuartzScheduler()
    {
        _scheduler = await _schedulerFactory.GetScheduler();
        await _scheduler.Start();
    }

    public Task AddDelayTask<T>(Guid taskId, Guid jobId, TimeSpan timeSpan) where T : IJob
    {
        var job = JobBuilder.Create<T>().
            WithIdentity(taskId.ToString(), jobId.ToString())
            .Build();

        job.JobDataMap.Add(ConstStrings.TASK_ID, taskId);
        job.JobDataMap.Add(ConstStrings.JOB_ID, jobId);

        var trigger = TriggerBuilder.Create()
            .WithIdentity(taskId.ToString(), jobId.ToString())
            .StartAt(DateTimeOffset.Now.Add(timeSpan))
            .Build();

        return _scheduler.ScheduleJob(job, trigger);
    }

    public async Task RegisterCronJob<T>(Guid jobId, string cron) where T : IJob
    {
        if (jobId == Guid.Empty)
        {
            throw new UserFriendlyException("JobId is empty");
        }

        if (string.IsNullOrEmpty(cron))
        {
            throw new UserFriendlyException("cron is empty");
        }

        if(!CronExpression.IsValidExpression(cron))
        {
            return;
        }

        var triggerKey = new TriggerKey(jobId.ToString());

        if(await _scheduler.CheckExists(triggerKey))
        {
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(jobId.ToString())
                .WithCronSchedule(cron)
                .Build();

            await _scheduler.RescheduleJob(triggerKey, newTrigger);
        }
        else
        {
            var job = JobBuilder.Create<T>().
                   WithIdentity(jobId.ToString())
                   .Build();

            job.JobDataMap.Add(ConstStrings.JOB_ID, jobId);

            var trigger = TriggerBuilder.Create()
                .WithIdentity(jobId.ToString())
                .WithCronSchedule(cron)
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
        }
    }

    public async Task RemoveCronJob(Guid jobId)
    {
        var triggerKey = new TriggerKey(jobId.ToString());

        if (await _scheduler.CheckExists(triggerKey))
        {
            await _scheduler.UnscheduleJob(triggerKey);
        }
    }

    public async Task RemoveDelayTask(Guid taskId, Guid jobId)
    {
        var triggerKey = new TriggerKey(taskId.ToString(), jobId.ToString());

        if (await _scheduler.CheckExists(triggerKey))
        {
            await _scheduler.UnscheduleJob(triggerKey);
        }
    }

    public Task<List<DateTimeOffset>> GetCronExcuteTimeByTimeRange(string cron, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        List<DateTimeOffset> excuteTimeList = new();

        var cronExpression = new CronExpression(cron);

        while (startTime < endTime)
        {
            var nextExcuteTime = cronExpression.GetNextValidTimeAfter(startTime);

            if (nextExcuteTime != null)
            {
                startTime = nextExcuteTime.Value;

                if(nextExcuteTime < endTime)
                {
                    excuteTimeList.Add(nextExcuteTime.Value.ToLocalTime());
                }
            }
        }

        return Task.FromResult(excuteTimeList);
    }
}
