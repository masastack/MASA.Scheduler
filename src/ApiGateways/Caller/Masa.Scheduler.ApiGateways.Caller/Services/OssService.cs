// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller.Services;

public class OssService : ServiceBase
{
    protected override string BaseUrl { get; set; }

    public OssService(ICaller provider) : base(provider)
    {
        BaseUrl = "api/oss";
    }

    public async Task<GetSecurityTokenDto> GetSecurityTokenAsync()
    {
        return await GetAsync<GetSecurityTokenDto>(nameof(GetSecurityTokenAsync));
    }
}
