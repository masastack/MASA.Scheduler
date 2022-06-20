// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Masa.Scheduler.Contracts.Server.Model;

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class SchedulerServerManagerService : ServiceBase
{
    protected override string BaseUrl { get; set; }

    public SchedulerServerManagerService(ICallerProvider provider): base(provider)
    {
        BaseUrl = "api/scheduler-server-manager/";
    }

    public async Task<List<WorkerModel>> GetWorkerListAsync()
    {
        var result = await GetAsync<List<WorkerModel>>(nameof(GetWorkerListAsync));
        return result ?? new();
    }
}

