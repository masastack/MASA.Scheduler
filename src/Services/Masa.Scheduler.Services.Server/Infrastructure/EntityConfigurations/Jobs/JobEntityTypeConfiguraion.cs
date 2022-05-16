﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Services.Server.Infrastructure.EntityConfigurations.Jobs;

public class JobEntityTypeConfiguraion : IEntityTypeConfiguration<SchedulerJob>
{
    public void Configure(EntityTypeBuilder<SchedulerJob> builder)
    {
        builder.ToTable(nameof(SchedulerJob), SchedulerDbContext.SERVER_SCHEMA);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.BelongTeamId);
        builder.HasIndex(x => x.BelongProjectId);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.Owner).HasMaxLength(20);
        builder.Property(x => x.Origin).HasMaxLength(50);
        builder.HasOne(x => x.RunDetail).WithOne().HasForeignKey<SchedulerJobRunDetail>(x => x.JobId);
        builder.Property(x => x.JobAppConfig).HasConversion(new JsonValueConverter<SchedulerJobAppConfig>());
        builder.Property(x => x.HttpConfig).HasConversion(new JsonValueConverter<SchedulerJobHttpConfig>());
        builder.Property(x => x.DaprServiceInvocationConfig).HasConversion(new JsonValueConverter<SchedulerJobDaprServiceInvocationConfig>());
    }
}
