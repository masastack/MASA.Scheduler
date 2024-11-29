﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Quartz.Impl.Matchers;

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

    public Task AddDelayTask<T>(string environment, Guid taskId, Guid jobId, TimeSpan timeSpan) where T : IJob
    {
        var job = JobBuilder.Create<T>().
            WithIdentity(taskId.ToString(), jobId.ToString())
            .Build();

        job.JobDataMap.Add(ConstStrings.TASK_ID, taskId);
        job.JobDataMap.Add(ConstStrings.JOB_ID, jobId);
        job.JobDataMap.Add(IsolationConsts.ENVIRONMENT, environment);

        var trigger = TriggerBuilder.Create()
            .WithIdentity(taskId.ToString(), jobId.ToString())
            .StartAt(DateTimeOffset.Now.Add(timeSpan))
            .Build();

        return _scheduler.ScheduleJob(job, trigger);
    }

    public async Task RegisterCronJob<T>(string environment, Guid jobId, string cron) where T : IJob
    {
        if (jobId == Guid.Empty)
        {
            throw new UserFriendlyException("JobId is empty");
        }

        if (string.IsNullOrEmpty(cron))
        {
            throw new UserFriendlyException("cron is empty");
        }

        if (!CronExpression.IsValidExpression(cron))
        {
            return;
        }

        var triggerKey = new TriggerKey(jobId.ToString());

        if (await _scheduler.CheckExists(triggerKey))
        {
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(jobId.ToString())
                .WithCronSchedule(cron, builder =>
                {
                    builder.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
                })
                .Build();

            await _scheduler.RescheduleJob(triggerKey, newTrigger);
        }
        else
        {
            var job = JobBuilder.Create<T>().
                   WithIdentity(jobId.ToString())
                   .Build();

            job.JobDataMap.Add(ConstStrings.JOB_ID, jobId);
            job.JobDataMap.Add(IsolationConsts.ENVIRONMENT, environment);

            var trigger = TriggerBuilder.Create()
                .WithIdentity(jobId.ToString())
                .WithCronSchedule(cron, builder =>
                {
                    builder.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
                })
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

        cronExpression.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

        while (startTime < endTime)
        {
            var nextExcuteTime = cronExpression.GetNextValidTimeAfter(startTime);

            if (nextExcuteTime != null)
            {
                startTime = nextExcuteTime.Value;

                if (nextExcuteTime < endTime)
                {
                    excuteTimeList.Add(nextExcuteTime.Value);
                }
            }
        }

        return Task.FromResult(excuteTimeList);
    }

    public async Task CleanAllJobsAsync()
    {
        // 获取所有触发器组名
        var triggerGroups = await _scheduler.GetTriggerGroupNames();

        // 遍历每个触发器组并删除其中的所有触发器
        foreach (var groupName in triggerGroups)
        {
            var triggers = await _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(groupName));
            foreach (var triggerKey in triggers)
            {
                await _scheduler.UnscheduleJob(triggerKey);
            }
        }

        // 获取所有作业组名
        var jobGroups = await _scheduler.GetJobGroupNames();

        // 遍历每个作业组并删除其中的所有作业
        foreach (var groupName in jobGroups)
        {
            var jobs = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName));
            foreach (var jobKey in jobs)
            {
                await _scheduler.DeleteJob(jobKey);
            }
        }
    }
}
