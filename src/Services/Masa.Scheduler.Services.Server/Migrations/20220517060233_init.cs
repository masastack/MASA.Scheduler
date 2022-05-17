﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "server");

            migrationBuilder.CreateTable(
                name: "IntegrationEventLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    TimesSent = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEventLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerJob",
                schema: "server",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsAlertException = table.Column<bool>(type: "bit", nullable: false),
                    ScheduleType = table.Column<int>(type: "int", nullable: false),
                    JobType = table.Column<int>(type: "int", nullable: false),
                    RoutingStrategy = table.Column<int>(type: "int", nullable: false),
                    ScheduleExpiredStrategy = table.Column<int>(type: "int", nullable: false),
                    ScheduleBlockStrategy = table.Column<int>(type: "int", nullable: false),
                    RunTimeoutStrategy = table.Column<int>(type: "int", nullable: false),
                    RunTimeoutSecond = table.Column<int>(type: "int", nullable: false),
                    FailedStrategy = table.Column<int>(type: "int", nullable: false),
                    FailedRetryInterval = table.Column<int>(type: "int", nullable: false),
                    FailedRetryCount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    BelongTeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BelongProjectId = table.Column<int>(type: "int", nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastScheduleTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastRunStartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastRunEndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastRunStatus = table.Column<int>(type: "int", nullable: false),
                    JobAppConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaprServiceInvocationConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HttpConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerJob", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerResource",
                schema: "server",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JobAppId = table.Column<int>(type: "int", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerResource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulerTask",
                schema: "server",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunCount = table.Column<int>(type: "int", nullable: false),
                    RunTime = table.Column<long>(type: "bigint", nullable: false),
                    TaskStatus = table.Column<int>(type: "int", nullable: false),
                    SchedulerStartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TaskRunStartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TaskRunEndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchedulerTask_SchedulerJob_JobId",
                        column: x => x.JobId,
                        principalSchema: "server",
                        principalTable: "SchedulerJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "index_eventid_version",
                table: "IntegrationEventLog",
                columns: new[] { "EventId", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "index_state_modificationtime",
                table: "IntegrationEventLog",
                columns: new[] { "State", "ModificationTime" });

            migrationBuilder.CreateIndex(
                name: "index_state_timessent_modificationtime",
                table: "IntegrationEventLog",
                columns: new[] { "State", "TimesSent", "ModificationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_BelongProjectId",
                schema: "server",
                table: "SchedulerJob",
                column: "BelongProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_BelongTeamId",
                schema: "server",
                table: "SchedulerJob",
                column: "BelongTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_Name",
                schema: "server",
                table: "SchedulerJob",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_Origin",
                schema: "server",
                table: "SchedulerJob",
                column: "Origin");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerResource_Name",
                schema: "server",
                table: "SchedulerResource",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask",
                column: "JobId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationEventLog");

            migrationBuilder.DropTable(
                name: "SchedulerResource",
                schema: "server");

            migrationBuilder.DropTable(
                name: "SchedulerTask",
                schema: "server");

            migrationBuilder.DropTable(
                name: "SchedulerJob",
                schema: "server");
        }
    }
}
