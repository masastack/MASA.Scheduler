// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class SchedulerTaskDto
{
    public Guid Id { get; set; }

    public int RunCount { get; set; }

    public long RunTime { get; set; }

    public TaskRunStatuses TaskStatus { get; set; }

    public DateTimeOffset SchedulerStartTime { get; set; }

    public DateTimeOffset TaskRunStartTime { get; set; } = DateTimeOffset.MinValue;

    public DateTimeOffset TaskRunEndTime { get; set; } = DateTimeOffset.MinValue;

    public Guid JobId { get; set; }

    public SchedulerJobDto Job { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public string Origin { get; set; } = string.Empty;

    public string WorkerHost { get; set; } = string.Empty;
}

