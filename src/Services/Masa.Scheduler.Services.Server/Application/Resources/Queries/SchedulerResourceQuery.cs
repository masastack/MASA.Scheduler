// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Resources.Queries;

public record SchedulerResourceQuery(SchedulerResourceListRequest Request) : Query<SchedulerResourceListResponse>
{
    public override SchedulerResourceListResponse Result { get; set; } = null!;
}
