// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public sealed record ScheduleJobResult(bool Success, string? Error)
{
    public static ScheduleJobResult SuccessResult() => new(true, null);

    public static ScheduleJobResult Fail(string? error) => new(false, error);
}

public sealed record CronActivationWindow(DateTimeOffset? StartingFrom, DateTimeOffset? Ttl)
{
    public static CronActivationWindow Empty { get; } = new(null, null);
}

public sealed class DaprJobPayload
{
    public DaprJobNameType Type { get; set; }
    public Guid JobId { get; set; }
    public Guid? TaskId { get; set; }
    public string Environment { get; set; } = string.Empty;
    public DateTimeOffset? ExecuteTime { get; set; }
    public string? CronExpression { get; set; }
    public string? CronTimeZone { get; set; }
}
