// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Services;

public class OssService : ServiceBase
{
    public OssService(IServiceCollection services) : base(services, ConstStrings.OSS_API)
    {
        MapGet(GetSecurityTokenAsync);
        MapGet(TestSecretStore);
    }

    private async Task<GetSecurityTokenDto> GetSecurityTokenAsync([FromServices] IClient client, [FromServices] DaprClient daprClient, [FromServices] IConfiguration configuration)
    {
        var region = "oss-cn-hangzhou";
        var response = client.GetSecurityToken();
        var stsToken = response.SessionToken;
        var accessId = response.AccessKeyId;
        var accessSecret = response.AccessKeySecret;

        var secretStoreName = configuration.GetValue<string>("SecretStoreName");
        var bucket = (await daprClient.GetSecretAsync(secretStoreName, "bucket")).First().Value;
        
        return await Task.FromResult(new GetSecurityTokenDto(region, accessId, accessSecret, stsToken, bucket));
    }

    private async Task<IResult> TestSecretStore(string storeName, [FromServices] DaprClient daprClient)
    {
        var result = await daprClient.GetBulkSecretAsync(storeName);
        foreach (var resultKey in result.Keys)
        {
            Console.WriteLine("*****************************");
            Console.WriteLine($"Result Key: {resultKey}");
            var value = result[resultKey];
            foreach (var key in value.Keys)
            {
                Console.WriteLine("------------------------------------");
                Console.WriteLine($"loadKey: {key}, Value: {value[key]}");
                Console.WriteLine("------------------------------------");
            }
            Console.WriteLine("*****************************");
        }
        return Results.Ok();
    }
}
