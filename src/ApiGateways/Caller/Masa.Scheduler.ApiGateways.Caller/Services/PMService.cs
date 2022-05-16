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
    public async Task<ProjectListResponse> GetProjectListAsync(Guid teamId)
    {
        var result = await GetAsync<List<ProjectDto>>($"{nameof(GetProjectListAsync)}?teamId={teamId}");

        var response = new ProjectListResponse()
        {
            Data = result ?? new List<ProjectDto>()
        };

        return response;
    }
}

