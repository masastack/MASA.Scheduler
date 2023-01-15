// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Masa.Scheduler.Services.Server.Infrastructure.Extensions;

public static class Extensions
{
    public static bool WildCardContains(this IEnumerable<string> data, string code)
    {
        return data.Any(item => Regex.IsMatch(code.ToLower(),
            Regex.Escape(item.ToLower()).Replace(@"\*", ".*").Replace(@"\?", ".")));
    }

    public static async Task MigrateDbContextAsync<TContext>(this WebApplicationBuilder builder,
        Func<TContext, IServiceProvider, Task> seeder) where TContext : DbContext
    {
        var services = builder.Services.BuildServiceProvider();
        var context = services.GetRequiredService<TContext>();
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }
        await seeder(context, services);
    }

    public static async Task MigrateDbContextAsync<TContext>(this WebApplicationBuilder builder) where TContext : DbContext
    {
        var services = builder.Services.BuildServiceProvider();
        var context = services.GetRequiredService<SchedulerDbContext>();
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
            //await SeedAsync(context, services);
        }
    }

    //public static async Task SeedAsync(SchedulerDbContext context, IServiceProvider serviceProvider)
    //{
    //    var path = "init-quartz-db.txt";
    //    if (!File.Exists("init-db.txt"))
    //    {
    //        Console.WriteLine("init-quartz-db.txt not exists, init db failed");
    //        return;
    //    }
    //    using var file = File.OpenText(path);
    //    var sql = file.ReadToEnd();
    //    if (string.IsNullOrEmpty(sql))
    //    {
    //        Console.WriteLine("init sql is empty");
    //        return;
    //    }

    //    _ = await context.Database.ExecuteSqlRawAsync(sql);
    //}
}
