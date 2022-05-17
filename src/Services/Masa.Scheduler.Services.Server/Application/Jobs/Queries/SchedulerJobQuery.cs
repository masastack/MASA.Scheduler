// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Application.Jobs.Queries;

public record SchedulerJobQuery(SchedulerJobListRequest Request) : Query<PaginationDto<SchedulerJobDto>>
{
    public override PaginationDto<SchedulerJobDto> Result { get; set; } = new();
}
