// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs.Queries;

public record SchedulerJobQuery(SchedulerJobListRequest Request) : Query<SchedulerJobListResponse>
{
    public override SchedulerJobListResponse Result { get; set; } = null!;
}
