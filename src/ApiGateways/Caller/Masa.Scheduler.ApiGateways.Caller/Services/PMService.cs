// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class PmService: ServiceBase
{
    protected override string BaseUrl { get; set; }

    public PmService(ICaller caller) : base(caller)
    {
        BaseUrl = "api/pm";
    }
    public async Task<ProjectListResponse> GetProjectListAsync(Guid? teamId, string environment = "")
    {
        var requestUrl = nameof(GetProjectListAsync) + $"?environment={environment}";

        if (teamId.HasValue)
        {
            requestUrl += $"&teamId={teamId.Value}";
        }

        var result = await GetAsync<List<ProjectDto>>(requestUrl);

        var response = new ProjectListResponse()
        {
            Data = result ?? new List<ProjectDto>()
        };

        return response;
    }
}

