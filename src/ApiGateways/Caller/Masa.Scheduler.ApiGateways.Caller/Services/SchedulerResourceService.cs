﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class SchedulerResourceService : ServiceBase
{
    protected override string BaseUrl { get; set; }

    public SchedulerResourceService(ICaller caller) : base(caller)
    {
        BaseUrl = "api/scheduler-resource/";
    }

    public async Task<SchedulerResourceListResponse> GetListAsync(SchedulerResourceListRequest request)
    {
        var result = await GetAsync<SchedulerResourceListRequest, SchedulerResourceListResponse>(nameof(GetListAsync), request);
        return result ?? new();
    }

    public async Task AddAsync(AddSchedulerResourceRequest request)
    {
        await PostAsync(string.Empty, request);
    }

    public async Task UpdateAsync(UpdateSchedulerResourceRequest request)
    {
        await PutAsync(string.Empty, request);
    }

    public async Task DeleteAsync(RemoveSchedulerResourceRequest request)
    {
        await DeleteAsync(string.Empty, request);
    }
}

