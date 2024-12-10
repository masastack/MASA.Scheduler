// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Worker.Infrastructure.Extensions;

public static class MasaDbContextBuilderExtensions
{
    public static MasaDbContextBuilder UseDbSql(this MasaDbContextBuilder builder, string dbType)
    {
        switch (dbType)
        {
            case "PostgreSql":
                builder.UseNpgsql();
                break;
            default:
                builder.UseSqlServer();
                break;
        }
        return builder;
    }
}
