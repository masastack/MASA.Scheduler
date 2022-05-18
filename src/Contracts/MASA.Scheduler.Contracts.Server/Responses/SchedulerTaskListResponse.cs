// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Responses;

public class SchedulerTaskListResponse : BasePaginationResponse<SchedulerTaskDto>
{
    public SchedulerTaskListResponse(long total, int totalPages, List<SchedulerTaskDto> result)
        : base(total, totalPages, result)
    {

    }
}

