// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.ApiGateways.Caller;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchedulerApiGateways(this IServiceCollection services, Action<SchedulerApiOptions>? configs = null)
    {
        var option = new SchedulerApiOptions();

        configs?.Invoke(option);
        services.AddSingleton(option);
        services.AddAutoRegistrationCaller(Assembly.Load("Masa.Scheduler.ApiGateways.Caller"));
        return services;
    }

    public static IServiceCollection AddJwtTokenValidator(this IServiceCollection services,
        Action<JwtTokenValidatorOptions> jwtTokenValidatorOptions, Action<ClientRefreshTokenOptions> clientRefreshTokenOptions)
    {
        var options = new JwtTokenValidatorOptions();
        jwtTokenValidatorOptions.Invoke(options);
        services.AddSingleton(options);
        var refreshTokenOptions = new ClientRefreshTokenOptions();
        clientRefreshTokenOptions.Invoke(refreshTokenOptions);
        services.AddSingleton(refreshTokenOptions);
        services.AddScoped<JwtTokenValidator>();
        return services;
    }
}
