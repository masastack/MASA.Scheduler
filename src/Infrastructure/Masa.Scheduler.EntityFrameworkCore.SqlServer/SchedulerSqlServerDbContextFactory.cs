// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.EntityFrameworkCore;

public class SchedulerSqlServerDbContextFactory : IDesignTimeDbContextFactory<SchedulerDbContext>
{
    public SchedulerDbContext CreateDbContext(string[] args)
    {
        SchedulerDbContext.RegisterAssembly(typeof(SchedulerSqlServerDbContextFactory).Assembly);
        var optionsBuilder = new MasaDbContextOptionsBuilder<SchedulerDbContext>();
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder
            .AddJsonFile("appsettings.SqlServer.json")
            .Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection")!, b => b.MigrationsAssembly("Masa.Scheduler.EntityFrameworkCore.SqlServer"));

        return new SchedulerDbContext(optionsBuilder.MasaOptions);
    }
}
