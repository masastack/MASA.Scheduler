// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Quartz;

public static class QuartzUtilsServiceCollectionExtensions
{
    public static IServiceCollection AddQuartzUtils(this IServiceCollection services, string quartzConnectString, string dbType)
    {
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.UsePersistentStore(config =>
            {
                if(dbType == "PostgreSql")
                {
                    config.UsePostgres(quartzConnectString);
                }
                else
                {
                    config.UseSqlServer(quartzConnectString);
                }
                config.UseClustering();
                config.UseJsonSerializer();
            });
            
        });
        services.AddQuartzServer(options =>
        {   
            options.WaitForJobsToComplete = true;
        });
        services.AddSingleton<QuartzUtils>();

        return services;
    }
}
