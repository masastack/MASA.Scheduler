// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class SchedulerTaskService: ServiceBase
{
    protected override string BaseUrl { get; set; }

    public SchedulerTaskService(ICallerProvider callerProvider) : base(callerProvider)
    {
        BaseUrl = "api/scheduler-task/";
    }

    public async Task<SchedulerTaskListResponse> GetListAsync(SchedulerTaskListRequest request)
    {
        var result = await GetAsync<SchedulerTaskListRequest, SchedulerTaskListResponse>(string.Empty, request);
        return result ?? new(0, 0, new());
    }

    public async Task StartAsync(StartSchedulerTaskRequest request)
    {
        await PutAsync(nameof(StartAsync), request);
    }

    public async Task StopAsync(StopSchedulerTaskRequest request)
    {
        await PutAsync(nameof(StopAsync), request);
    }

    public async Task RemoveAsync(RemoveSchedulerTaskRequest request)
    {
        await DeleteAsync(string.Empty, request);
    }
}
