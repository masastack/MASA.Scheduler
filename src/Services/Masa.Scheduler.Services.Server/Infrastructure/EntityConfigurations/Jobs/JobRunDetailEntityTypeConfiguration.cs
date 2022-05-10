﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masa.Scheduler.Services.Server.Infrastructure.EntityConfigurations.Jobs;

public class JobRunDetailEntityTypeConfiguration : IEntityTypeConfiguration<SchedulerJobRunDetail>
{
    public void Configure(EntityTypeBuilder<SchedulerJobRunDetail> builder)
    {
        builder.ToTable(nameof(SchedulerJobRunDetail), SchedulerDbContext.SERVER_SCHEMA);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.JobId).IsUnique();
    }
}
