// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Dtos;

public class PaginationDto<T>
{
    public long Total { get; set; }

    public int TotalPages { get; set; }

    public List<T> Items { get; set; } = default!;

    public PaginationDto()
    {
        Items = new List<T>();
    }

    public PaginationDto(long total, int totalPages, List<T> result)
    {
        Total = total;
        TotalPages = totalPages;
        Items = result;
    }
}
