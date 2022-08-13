// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Scheduler.Contracts.Server.Infrastructure.Logger;

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class SchedulerTaskDto
{
    public Guid Id { get; set; }

    public int RunCount { get; set; }

    /// <summary>
    /// Task run use total time (second)
    /// </summary>
    public long RunTime { get; set; }

    public TaskRunStatus TaskStatus { get; set; }

    public DateTimeOffset SchedulerTime { get; set; }

    public DateTimeOffset TaskRunStartTime { get; set; } = DateTimeOffset.MinValue;

    public DateTimeOffset TaskRunEndTime { get; set; } = DateTimeOffset.MinValue;

    public Guid JobId { get; set; }

    public SchedulerJobDto Job { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public string Origin { get; set; } = string.Empty;

    public string WorkerHost { get; set; } = string.Empty;

    public Guid OperatorId { get; set; }

    public string OperatorName { get; set; } = String.Empty;

    public DateTime CreationTime { get; set; }

    public SchedulerLogger? Logger { get; set; } 
}
