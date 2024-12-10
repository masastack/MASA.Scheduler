// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.EntityFrameworkCore;

public class SchedulerPostgreSqlDbContextFactory : IDesignTimeDbContextFactory<SchedulerDbContext>
{
    public SchedulerDbContext CreateDbContext(string[] args)
    {
        SchedulerDbContext.RegisterAssembly(typeof(SchedulerPostgreSqlDbContextFactory).Assembly);
        var optionsBuilder = new MasaDbContextOptionsBuilder<SchedulerDbContext>();
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder
            .AddJsonFile("appsettings.PostgreSql.json")
            .Build();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection")!, b => b.MigrationsAssembly("Masa.Scheduler.EntityFrameworkCore.PostgreSql"));

        return new SchedulerDbContext(optionsBuilder.MasaOptions);
    }
}
