// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class PMService: ServiceBase
{
    protected override string BaseUrl { get; set; }

    public PMService(ICallerProvider provider) : base(provider)
    {
        BaseUrl = "api/pm";
    }
    public async Task<List<ProjectModel>> GetProjectListAsync(Guid teamId)
    {
        var result = await GetAsync<List<ProjectModel>>($"{nameof(GetProjectListAsync)}?teamId={teamId}");
        return result ?? new List<ProjectModel>();
    }
}

