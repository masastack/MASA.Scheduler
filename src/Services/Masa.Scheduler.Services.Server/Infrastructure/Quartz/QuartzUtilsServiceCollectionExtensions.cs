// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Quartz;

public static class QuartzUtilsServiceCollectionExtensions
{
    public static IServiceCollection AddQuartzUtils(this IServiceCollection services, string quartzConnectString)
    {
        services.AddQuartz(q =>
        {
            q.UsePersistentStore(config =>
            {
                config.UseSqlServer(quartzConnectString);
                config.UseClustering();
                config.UseNewtonsoftJsonSerializer();
            });
            q.UseDefaultThreadPool(20);
        });
        services.AddQuartzServer(options =>
        {   
            options.WaitForJobsToComplete = true;
        });
        services.AddSingleton<QuartzUtils>();

        return services;
    }
}
