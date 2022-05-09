// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure;

public class SchedulerDbContext : IsolationDbContext
{
    public const string SERVER_SCHEMA = "server";

    public DbSet<Job> Jobs { get; set; } = default!;

    public SchedulerDbContext(MasaDbContextOptions<SchedulerDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    protected override void OnModelCreatingExecuting(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreatingExecuting(builder);
    }
}
