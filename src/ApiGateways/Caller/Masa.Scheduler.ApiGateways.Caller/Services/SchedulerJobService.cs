// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class SchedulerJobService : ServiceBase
{
    protected override string BaseUrl { get; set; }

    public SchedulerJobService(ICallerProvider callerProvider) : base(callerProvider)
    {
        BaseUrl = "api/scheduler-job/";
    }

    public async Task<List<SchedulerJobDto>> GetListAsync()
    {
        var result = await GetAsync<List<SchedulerJobDto>>($"List");
        return result ?? new List<SchedulerJobDto>();
    }
}
