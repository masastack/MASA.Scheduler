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
        services.AddScoped<HttpClientAuthorizationDelegatingHandler>();
        services.AddCaller(Assembly.Load("Masa.Scheduler.ApiGateways.Caller"));
        return services;
    }
}
