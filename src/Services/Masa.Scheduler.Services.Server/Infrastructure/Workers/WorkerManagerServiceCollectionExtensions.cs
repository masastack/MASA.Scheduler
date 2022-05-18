// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Workers;

public static class WorkerManagerServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerManager(this IServiceCollection services)
    {
        services.AddSingleton<WorkerManager>();

        return services;
    }
}
