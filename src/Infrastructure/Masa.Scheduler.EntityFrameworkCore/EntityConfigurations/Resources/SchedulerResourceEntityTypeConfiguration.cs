﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.EntityFrameworkCore.EntityConfigurations.Resources;

public class SchedulerResourceEntityTypeConfiguration : IEntityTypeConfiguration<SchedulerResource>
{
    public void Configure(EntityTypeBuilder<SchedulerResource> builder)
    {
        builder.ToTable(nameof(SchedulerResource), SchedulerDbContext.SERVER_SCHEMA);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Name);
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.FilePath).HasMaxLength(255);
        builder.Property(x => x.Version).HasMaxLength(20);
        builder.Property(x => x.JobAppIdentity).HasMaxLength(100);
    }
}
