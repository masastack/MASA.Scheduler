// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.TaskHandlers;

public interface ITaskHandler
{
    Task<TaskRunStatus> RunTask(Guid taskId, SchedulerJobDto schedulerJobDto, DateTimeOffset excuteTime, string? traceId, string? spanId, CancellationToken token);
}
