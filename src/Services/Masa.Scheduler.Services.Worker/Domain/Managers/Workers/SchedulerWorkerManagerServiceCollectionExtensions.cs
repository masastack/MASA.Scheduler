// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Managers.Workers;

public static class SchedulerWorkerManagerServiceCollectionExtensions
{
    public static IServiceCollection AddServerManager(this IServiceCollection services)
    {
        services.AddScoped<SchedulerWorkerManager>();
        services.AddScoped<TaskHanlderFactory>();
        services.AddScoped<HttpTaskHandler>();
        services.AddScoped<JobAppTaskHandler>();
        services.AddScoped<DaprServiceInvocationTaskHanlder>();
        services.AddSingleton<SchedulerWorkerManagerData>();
        services.AddHostedService<SchedulerWorkerManagerHostService>();

        return services;
    }
}
