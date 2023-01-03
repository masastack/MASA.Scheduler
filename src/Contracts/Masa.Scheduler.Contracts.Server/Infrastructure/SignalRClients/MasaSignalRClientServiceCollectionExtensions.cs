// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.SignalRClients;

public static class MasaSignalRClientServiceCollectionExtensions
{
    public static IServiceCollection AddMasaSignalRClient(this IServiceCollection services, Action<MasaSignalROptions> options)
    {
        var option = new MasaSignalROptions();
        options?.Invoke(option);
        services.AddSingleton(option);
        services.AddScoped<MasaSignalRClient>();
        return services;
    }
}

