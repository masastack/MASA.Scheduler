// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class OssService : ServiceBase
{
    public OssService() : base(ConstStrings.OSS_API)
    {

    }

    public async Task<SecurityTokenDto> GetSecurityTokenAsync([FromServices] IObjectStorageClient client, [FromServices] IOptions<OssOptions> ossOptions)
    {
        var region = "oss-cn-hangzhou";
        var response = client.GetSecurityToken();
        var stsToken = response.SessionToken;
        var accessId = response.AccessKeyId;
        var accessSecret = response.AccessKeySecret;
        var bucket = ossOptions.Value.Bucket;
        return await Task.FromResult(new SecurityTokenDto(region, accessId, accessSecret, stsToken, bucket));
    }
}
