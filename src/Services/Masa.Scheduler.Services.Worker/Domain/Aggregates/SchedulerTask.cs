// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Domain.Aggregates;

public class SchedulerTask : FullAggregateRoot<Guid, Guid>
{
    public int RunCount { get; private set; }

    /// <summary>
    /// Task run use total time (second)
    /// </summary>
    public long RunTime { get; private set; }
}
