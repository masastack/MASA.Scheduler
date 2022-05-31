﻿// <auto-generated />
using System;
using Masa.Scheduler.Services.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    [DbContext(typeof(SchedulerDbContext))]
    partial class SchedulerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Masa.BuildingBlocks.Dispatcher.IntegrationEvents.Logs.IntegrationEventLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EventTypeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ModificationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("RowVersion")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("nvarchar(36)");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.Property<int>("TimesSent")
                        .HasColumnType("int");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "EventId", "RowVersion" }, "index_eventid_version");

                    b.HasIndex(new[] { "State", "ModificationTime" }, "index_state_modificationtime");

                    b.HasIndex(new[] { "State", "TimesSent", "ModificationTime" }, "index_state_timessent_modificationtime");

                    b.ToTable("IntegrationEventLog", (string)null);
                });

            modelBuilder.Entity("Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.SchedulerJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BelongProjectId")
                        .HasColumnType("int");

                    b.Property<Guid>("BelongTeamId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Creator")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CronExpression")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("DaprServiceInvocationConfig")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("Enabled")
                        .HasColumnType("bit");

                    b.Property<int>("FailedRetryCount")
                        .HasColumnType("int");

                    b.Property<int>("FailedRetryInterval")
                        .HasColumnType("int");

                    b.Property<int>("FailedStrategy")
                        .HasColumnType("int");

                    b.Property<string>("HttpConfig")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAlertException")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("JobAppConfig")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("JobType")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("LastRunEndTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("LastRunStartTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("LastRunStatus")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("LastScheduleTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTime>("ModificationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Modifier")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Origin")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("RoutingStrategy")
                        .HasColumnType("int");

                    b.Property<int>("RunTimeoutSecond")
                        .HasColumnType("int");

                    b.Property<int>("RunTimeoutStrategy")
                        .HasColumnType("int");

                    b.Property<int>("ScheduleBlockStrategy")
                        .HasColumnType("int");

                    b.Property<int>("ScheduleExpiredStrategy")
                        .HasColumnType("int");

                    b.Property<int>("ScheduleType")
                        .HasColumnType("int");

                    b.Property<string>("SpecifiedWorkerHost")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("BelongProjectId");

                    b.HasIndex("BelongTeamId");

                    b.HasIndex("Name");

                    b.HasIndex("Origin");

                    b.ToTable("SchedulerJob", "server");
                });

            modelBuilder.Entity("Masa.Scheduler.Services.Server.Domain.Aggregates.Resources.SchedulerResource", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Creator")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<int>("JobAppId")
                        .HasColumnType("int");

                    b.Property<DateTime>("ModificationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Modifier")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("SchedulerResource", "server");
                });

            modelBuilder.Entity("Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks.SchedulerTask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Creator")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<Guid>("JobId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime>("ModificationTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Modifier")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Origin")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("RunCount")
                        .HasColumnType("int");

                    b.Property<long>("RunTime")
                        .HasColumnType("bigint");

                    b.Property<Guid>("RunUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("SchedulerTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("TaskRunEndTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("TaskRunStartTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("TaskStatus")
                        .HasColumnType("int");

                    b.Property<string>("WorkerHost")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.ToTable("SchedulerTask", "server");
                });

            modelBuilder.Entity("Masa.Scheduler.Services.Server.Domain.Aggregates.Tasks.SchedulerTask", b =>
                {
                    b.HasOne("Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.SchedulerJob", "Job")
                        .WithMany("SchedulerTasks")
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("Masa.Scheduler.Services.Server.Domain.Aggregates.Jobs.SchedulerJob", b =>
                {
                    b.Navigation("SchedulerTasks");
                });
#pragma warning restore 612, 618
        }
    }
}
