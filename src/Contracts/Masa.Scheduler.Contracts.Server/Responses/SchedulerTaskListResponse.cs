// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Responses;

public class SchedulerTaskListResponse : BasePaginationResponse<SchedulerTaskDto>
{
    public List<string> OriginList { get; set; }
    public SchedulerTaskListResponse()
        :base(0, 0, new List<SchedulerTaskDto>())
    {
        OriginList = new();
    }

    public SchedulerTaskListResponse(long total, int totalPages, List<SchedulerTaskDto> result, List<string> originList)
        : base(total, totalPages, result)
    {
        OriginList = originList;
    }
}
