// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Managers.Workers.Data;

public class TaskRunModel
{
    public Guid TaskId { get; set; }

    public DateTimeOffset ExcuteTime { get; set; }

    public SchedulerJobDto Job { get; set; } = default!;

    public string ServiceId { get; set; } = string.Empty;

    public string? TraceId { get; set; }

    public string? SpanId { get; set; }
}
