// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masa.Scheduler.Services.Server.Infrastructure.EntityConfigurations.Jobs;

public class JobRunDetailEntityTypeConfiguration : IEntityTypeConfiguration<JobRunDetail>
{
    public void Configure(EntityTypeBuilder<JobRunDetail> builder)
    {
        builder.ToTable(nameof(JobRunDetail), SchedulerDbContext.SERVER_SCHEMA);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.JobId).IsUnique();
    }
}
