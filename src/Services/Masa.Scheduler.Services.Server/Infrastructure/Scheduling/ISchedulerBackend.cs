// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public interface ISchedulerBackend
{
    Task StartAsync();

    Task RegisterCronJob(string environment, Guid jobId, string cron);

    Task RemoveCronJob(Guid jobId);

    Task AddDelayTask(string environment, Guid taskId, Guid jobId, TimeSpan delay);

    Task RemoveDelayTask(Guid taskId, Guid jobId);

    Task<List<DateTimeOffset>> GetCronExecuteTimeByTimeRange(string cron, DateTimeOffset startTime, DateTimeOffset endTime);
}
