// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.SignalR;

public static class SignalRServiceCollectionExtensions
{
    public static IServiceCollection AddMasaSignalR(this IServiceCollection services)
    {
        services.AddTransient<IUserIdProvider, MasaUserIdProvider>();
        services.AddTransient<NotificationsHub>();
        services.AddSignalR();

        return services;
    }
}
