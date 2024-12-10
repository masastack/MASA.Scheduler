// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.EntityFrameworkCore.EntityConfigurations.Tasks;

public class SchedulerTaskEntityTypeConfiguration : IEntityTypeConfiguration<SchedulerTask>
{
    public void Configure(EntityTypeBuilder<SchedulerTask> builder)
    {
        builder.ToTable(nameof(SchedulerTask), SchedulerDbContext.SERVER_SCHEMA);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.JobId, x.IsDeleted, x.TaskStatus });
        builder.HasIndex(x => new { x.Origin, x.IsDeleted });
        builder.Property(x => x.Origin).HasMaxLength(50);
        builder.Property(x => x.WorkerHost).HasMaxLength(100);
        builder.Property(x => x.Message);
        builder.Property(x => x.TraceId).HasMaxLength(255);
        builder.HasOne(x => x.Job).WithMany(p => p.SchedulerTasks).HasForeignKey(x => x.JobId);
    }
}
