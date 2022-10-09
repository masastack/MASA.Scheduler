// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class OssService : ServiceBase
{
    public OssService() : base(ConstStrings.OSS_API)
    {
    }

    public async Task<GetSecurityTokenDto> GetSecurityTokenAsync([FromServices] IClient client, [FromServices] DaprClient daprClient)
    {
        var region = "oss-cn-hangzhou";
        var response = client.GetSecurityToken();
        var stsToken = response.SessionToken;
        var accessId = response.AccessKeyId;
        var accessSecret = response.AccessKeySecret;

        var secrets = await daprClient.GetSecretAsync("localsecretstore", "masa-scheduler-secret");
        var bucket = secrets.GetValueOrDefault("bucket", string.Empty);

        return await Task.FromResult(new GetSecurityTokenDto(region, accessId, accessSecret, stsToken, bucket));
    }
}
