// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Scheduling;

public static class SchedulerBackendServiceCollectionExtensions
{
    public static IServiceCollection AddSchedulerBackend(this IServiceCollection services, IConfiguration configuration, string quartzConnectString, string dbType)
    {
        var schedulerSection = configuration.GetSection(SchedulerBackendOptions.SectionName);
        services.Configure<SchedulerBackendOptions>(schedulerSection);

        var backend = schedulerSection.GetValue<string>(nameof(SchedulerBackendOptions.Backend));
        var cleanupOtherBackend = schedulerSection.GetValue<bool>(nameof(SchedulerBackendOptions.CleanupOtherBackendOnStart));
        var useDaprJobs = string.Equals(backend, SchedulerBackendType.DaprJobs, StringComparison.OrdinalIgnoreCase);

        if (!useDaprJobs || cleanupOtherBackend)
        {
            services.AddQuartzUtils(quartzConnectString, dbType, enableServer: !useDaprJobs);
            if (!useDaprJobs)
            {
                services.AddScoped<QuartzSchedulerBackend>();
            }
        }

        services.AddScoped<DaprJobsSchedulerBackend>();
        services.AddScoped<ISchedulerBackend>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SchedulerBackendOptions>>().Value;
            var useDapr = string.Equals(options.Backend, SchedulerBackendType.DaprJobs, StringComparison.OrdinalIgnoreCase);
            return useDapr
                ? sp.GetRequiredService<DaprJobsSchedulerBackend>()
                : sp.GetRequiredService<QuartzSchedulerBackend>();
        });

        return services;
    }
}
