// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Responses;

public class BasePaginationResponse<T>: BaseResponse<List<T>>
{
    public long Total { get; set; }

    public int TotalPages { get; set; }

    public BasePaginationResponse()
    {
        Data = new List<T>();
    }

    public BasePaginationResponse(long total, int totalPages, List<T> result)
    {
        Total = total;
        TotalPages = totalPages;
        Data = result;
    }
}

