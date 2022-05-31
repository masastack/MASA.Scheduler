// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Domain.Managers.Servers;

public static class SchedulerServerManagerServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerManager(this IServiceCollection services)
    {
        services.AddScoped<SchedulerServerManager>();
        services.AddSingleton<SchedulerServerManagerData>();
        
        services.AddHostedService<SchedulerServerManagerBackgroundService>();

        return services;
    }
}
