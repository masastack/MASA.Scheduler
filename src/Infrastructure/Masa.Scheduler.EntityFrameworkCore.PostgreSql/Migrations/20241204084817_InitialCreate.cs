using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "server");

            migrationBuilder.CreateTable(
                name: "IntegrationEventLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTypeName = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    TimesSent = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ExpandContent = table.Column<string>(type: "text", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Owner = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAlertException = table.Column<bool>(type: "boolean", nullable: false),
                    ScheduleType = table.Column<int>(type: "integer", nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JobIdentity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JobType = table.Column<int>(type: "integer", nullable: false),
                    RoutingStrategy = table.Column<int>(type: "integer", nullable: false),
                    SpecifiedWorkerHost = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScheduleExpiredStrategy = table.Column<int>(type: "integer", nullable: false),
                    ScheduleBlockStrategy = table.Column<int>(type: "integer", nullable: false),
                    RunTimeoutStrategy = table.Column<int>(type: "integer", nullable: false),
                    RunTimeoutSecond = table.Column<int>(type: "integer", nullable: false),
                    FailedStrategy = table.Column<int>(type: "integer", nullable: false),
                    FailedRetryInterval = table.Column<int>(type: "integer", nullable: false),
                    FailedRetryCount = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    BelongTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    BelongProjectIdentity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Origin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NotifyUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AlarmRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateExpiredStrategyTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastScheduleTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastRunStartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastRunEndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastRunStatus = table.Column<int>(type: "integer", nullable: false),
                    JobAppConfig = table.Column<string>(type: "text", nullable: false),
                    DaprServiceInvocationConfig = table.Column<string>(type: "text", nullable: false),
                    HttpConfig = table.Column<string>(type: "text", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modifier = table.Column<Guid>(type: "uuid", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JobAppIdentity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modifier = table.Column<Guid>(type: "uuid", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunCount = table.Column<int>(type: "integer", nullable: false),
                    RunTime = table.Column<long>(type: "bigint", nullable: false),
                    TraceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TaskStatus = table.Column<int>(type: "integer", nullable: false),
                    SchedulerTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TaskRunStartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TaskRunEndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Origin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkerHost = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Creator = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modifier = table.Column<Guid>(type: "uuid", nullable: false),
                    ModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "IX_EventId_Version",
                table: "IntegrationEventLog",
                columns: new[] { "EventId", "RowVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_State_MTime",
                table: "IntegrationEventLog",
                columns: new[] { "State", "ModificationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_State_TimesSent_MTime",
                table: "IntegrationEventLog",
                columns: new[] { "State", "TimesSent", "ModificationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_BelongProjectIdentity",
                schema: "server",
                table: "SchedulerJob",
                column: "BelongProjectIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_BelongTeamId",
                schema: "server",
                table: "SchedulerJob",
                column: "BelongTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_Name",
                schema: "server",
                table: "SchedulerJob",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_Origin",
                schema: "server",
                table: "SchedulerJob",
                column: "Origin");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerResource_Name",
                schema: "server",
                table: "SchedulerResource",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId_IsDeleted_TaskStatus",
                schema: "server",
                table: "SchedulerTask",
                columns: new[] { "JobId", "IsDeleted", "TaskStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_Origin_IsDeleted",
                schema: "server",
                table: "SchedulerTask",
                columns: new[] { "Origin", "IsDeleted" });
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
