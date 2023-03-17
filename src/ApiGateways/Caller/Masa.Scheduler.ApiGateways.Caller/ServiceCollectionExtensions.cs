// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchedulerApiGateways(this IServiceCollection services, Action<SchedulerApiOptions>? configs = null)
    {
        services.AddSingleton<IResponseMessage, SchedulerResponseMessage>();
        var options = new SchedulerApiOptions();
        configs?.Invoke(options);
        services.AddSingleton(options);
        services.AddStackCaller(Assembly.Load("Masa.Scheduler.ApiGateways.Caller"), (serviceProvider) => { return new TokenProvider(); }, jwtTokenValidatorOptions =>
        {
            jwtTokenValidatorOptions.AuthorityEndpoint = options.AuthorityEndpoint;
        }, clientRefreshTokenOptions =>
        {
            clientRefreshTokenOptions.ClientId = options.ClientId;
            clientRefreshTokenOptions.ClientSecret = options.ClientSecret;
        });
        return services;
    }
}
