// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Tasks.Queries;

public record SchedulerTaskQuery(SchedulerTaskListRequest Request) : Query<SchedulerTaskListResponse>
{
    public override SchedulerTaskListResponse Result { get; set; } = null!;
}
