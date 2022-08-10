// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Responses;

public class SchedulerJobListResponse : BasePaginationResponse<SchedulerJobDto>
{
    public List<string> OriginList { get; set; }
    public SchedulerJobListResponse()
        : base(0, 0, new List<SchedulerJobDto>())
    {
        OriginList = new();
    }

    public SchedulerJobListResponse(long total, int totalPages, List<SchedulerJobDto> result, List<string> originList)
        :base(total, totalPages, result)
    {
        OriginList = originList;
    }
}

