// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Contracts.Server.Infrastructure.Logger;

public static class SchedulerLoggerServiceCollectionExtensions
{
    public static IServiceCollection AddSchedulerLogger(this IServiceCollection services)
    {
        services.AddScoped<SchedulerLogger>();
        return services;
    }
}
