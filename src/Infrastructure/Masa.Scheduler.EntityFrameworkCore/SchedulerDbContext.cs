// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.EntityFrameworkCore;

public class SchedulerDbContext : MasaDbContext<SchedulerDbContext>
{
    internal static Assembly Assembly = typeof(SchedulerDbContext).Assembly;

    public const string SERVER_SCHEMA = "server";

    public DbSet<SchedulerJob> Jobs { get; set; } = default!;

    public DbSet<SchedulerTask> Tasks { get; set; } = default!;

    public DbSet<SchedulerResource> Resources { get; set; } = default!;

    public SchedulerDbContext(MasaDbContextOptions<SchedulerDbContext> options) : base(options)
    {
        base.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
    }

    protected override void OnConfiguring(MasaDbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.DbContextOptionsBuilder
            .LogTo(Console.WriteLine, LogLevel.Warning)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    protected override void OnModelCreatingExecuting(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        builder.ApplyConfigurationsFromAssembly(typeof(IntegrationEventLogModelCreatingProvider).Assembly);
        builder.ApplyConfigurationsFromAssembly(Assembly);

        // Apply provider-specific configurations
        ApplyProviderSpecificConfigurations(builder);

        base.OnModelCreatingExecuting(builder);
    }

    public static void RegisterAssembly(Assembly assembly)
    {
        Assembly = assembly;
    }

    private void ApplyProviderSpecificConfigurations(ModelBuilder builder)
    {
        if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(new DateTimeUtcConverter());
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new NullableDateTimeUtcConverter());
                    }
                }
            }
        }
    }
}
