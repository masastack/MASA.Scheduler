// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Managers.Workers;

public static class SchedulerWorkerManagerServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerManager(this IServiceCollection services)
    {
        services.AddScoped<SchedulerWorkerManager>();
        services.AddTransient<TaskHanlderFactory>();
        services.AddTransient<HttpTaskHandler>();
        services.AddTransient<JobAppTaskHandler>();
        services.AddTransient<DaprServiceInvocationTaskHanlder>();
        services.AddSingleton<SchedulerWorkerManagerData>();
        services.AddHostedService<SchedulerWorkerManagerHostService>();
        services.AddScoped<IScopedProcessingService, WorkerScopedProcessingService>();
        return services;
    }
}
