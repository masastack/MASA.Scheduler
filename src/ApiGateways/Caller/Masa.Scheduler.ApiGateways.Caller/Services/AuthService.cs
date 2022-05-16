﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class AuthService : ServiceBase
{
    protected override string BaseUrl { get; set; }

    public AuthService(ICallerProvider provider) : base(provider)
    {
        BaseUrl = "api/auth";
    }
    public async Task<TeamListResponse> GetTeamListAsync()
    {
        var result = await GetAsync<List<TeamDto>>($"GetTeamList");

        var response = new TeamListResponse()
        {
            Data = result ?? new()
        };

        return response;
    }
}
