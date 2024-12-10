// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection;

public static class SeedData
{
    public static async Task MigratePostgreSqlAsync(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var context = serviceProvider.GetRequiredService<SchedulerDbContext>();

        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
            await SeedAsync(context, serviceProvider);
        }
    }

    public static async Task SeedAsync(SchedulerDbContext context, IServiceProvider serviceProvider)
    {
        try
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var path = $"{basePath}qrtz-postgres-sql.txt";
            if (!File.Exists(path))
            {
                Console.WriteLine($"{path} not exists, init db failed");
                return;
            }
            using var file = File.OpenText(path);
            var sql = file.ReadToEnd();
            if (string.IsNullOrEmpty(sql))
            {
                Console.WriteLine("init sql is empty");
                return;
            }

            _ = await context.Database.ExecuteSqlRawAsync(sql);
            Console.WriteLine("db init success");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"db init fail: message:{ex.Message},stacktrace:{ex.StackTrace}");
        }

    }
}
