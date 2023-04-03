// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class SchedulerJobService : ServiceBase
{
    protected override string BaseUrl { get; set; }

    public SchedulerJobService(ICaller caller) : base(caller)
    {
        BaseUrl = "api/scheduler-job/";
    }

    public async Task<SchedulerJobListResponse> GetListAsync(SchedulerJobListRequest request)
    {
        var result = await GetAsync<SchedulerJobListRequest,SchedulerJobListResponse>(nameof(GetListAsync), request);
        return result ?? new();
    }

    public async Task AddAsync(AddSchedulerJobRequest request)
    {
        await PostAsync(string.Empty, request);
    }

    public async Task UpdateAsync(UpdateSchedulerJobRequest request)
    {
        await PutAsync(string.Empty, request);
    }

    public async Task DeleteAsync(RemoveSchedulerJobRequest request)
    {
        await DeleteAsync(string.Empty, request);
    }

    public async Task ChangeEnableStatusAsync(ChangeEnabledStatusRequest request)
    {
        await PutAsync(nameof(ChangeEnableStatusAsync), request);
    }

    public async Task StartJobAsync(StartSchedulerJobRequest request)
    {
        await PutAsync(nameof(StartJobAsync), request);
    }

    public async Task UpsertAlarmRuleAsync(Guid id, Guid alarmRuleId)
    {
        await PostAsync($"{id}/upsert-alarm?alarmRuleId={alarmRuleId}", new { });
    }
}
