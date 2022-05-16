﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masa.Scheduler.Services.Server.Infrastructure.EntityConfigurations.Tasks;

public class SchedulerTaskEntityTypeConfiguration : IEntityTypeConfiguration<SchedulerTask>
{
    public void Configure(EntityTypeBuilder<SchedulerTask> builder)
    {
        builder.ToTable(nameof(SchedulerTask), SchedulerDbContext.SERVER_SCHEMA);
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Job).WithMany(p => p.SchedulerTasks).HasForeignKey(x => x.JobId);
    }
}
