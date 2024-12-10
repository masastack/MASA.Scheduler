// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.Extensions;

public static class MasaDbContextBuilderExtensions
{
    public static MasaDbContextBuilder UseDbSql(this MasaDbContextBuilder builder, string dbType)
    {
        switch (dbType)
        {
            case "PostgreSql":
                SchedulerDbContext.RegisterAssembly(typeof(SchedulerPostgreSqlDbContextFactory).Assembly);
                builder.UseNpgsql(b => b.MigrationsAssembly("Masa.Scheduler.EntityFrameworkCore.PostgreSql"));
                break;
            default:
                SchedulerDbContext.RegisterAssembly(typeof(SchedulerSqlServerDbContextFactory).Assembly);
                builder.UseSqlServer(b => b.MigrationsAssembly("Masa.Scheduler.EntityFrameworkCore.SqlServer"));
                break;
        }
        return builder;
    }
}
