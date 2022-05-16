using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations;

public partial class renameJobTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_SchedulerTask_Job_JobId",
            schema: "server",
            table: "SchedulerTask");

        migrationBuilder.DropTable(
            name: "JobRunDetail",
            schema: "server");

        migrationBuilder.DropTable(
            name: "Job",
            schema: "server");

        migrationBuilder.DropColumn(
            name: "DownloadUrl",
            schema: "server",
            table: "SchedulerResource");

        migrationBuilder.RenameColumn(
            name: "ResourceType",
            schema: "server",
            table: "SchedulerResource",
            newName: "JobAppId");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TaskRunStartTime",
            schema: "server",
            table: "SchedulerTask",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TaskRunEndTime",
            schema: "server",
            table: "SchedulerTask",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "SchedulerStartTime",
            schema: "server",
            table: "SchedulerTask",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<string>(
            name: "Version",
            schema: "server",
            table: "SchedulerResource",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "SchedulerJob",
            schema: "server",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Principal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IsAlertException = table.Column<bool>(type: "bit", nullable: false),
                AlertMessageTemplate = table.Column<int>(type: "int", nullable: false),
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
                Status = table.Column<int>(type: "int", nullable: false),
                BelongTeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BelongProjectId = table.Column<int>(type: "int", nullable: false),
                ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MainFunc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
            name: "SchedulerJobRunDetail",
            schema: "server",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SuccessCount = table.Column<int>(type: "int", nullable: false),
                FailureCount = table.Column<int>(type: "int", nullable: false),
                TimeoutCount = table.Column<int>(type: "int", nullable: false),
                TimeoutSuccessCount = table.Column<int>(type: "int", nullable: false),
                TimeoutFailureCount = table.Column<int>(type: "int", nullable: false),
                TotalRunCount = table.Column<int>(type: "int", nullable: false),
                LastRunTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastRunStatus = table.Column<int>(type: "int", nullable: false),
                JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SchedulerJobRunDetail", x => x.Id);
                table.ForeignKey(
                    name: "FK_SchedulerJobRunDetail_SchedulerJob_JobId",
                    column: x => x.JobId,
                    principalSchema: "server",
                    principalTable: "SchedulerJob",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

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
            name: "IX_SchedulerJobRunDetail_JobId",
            schema: "server",
            table: "SchedulerJobRunDetail",
            column: "JobId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_SchedulerTask_SchedulerJob_JobId",
            schema: "server",
            table: "SchedulerTask",
            column: "JobId",
            principalSchema: "server",
            principalTable: "SchedulerJob",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_SchedulerTask_SchedulerJob_JobId",
            schema: "server",
            table: "SchedulerTask");

        migrationBuilder.DropTable(
            name: "SchedulerJobRunDetail",
            schema: "server");

        migrationBuilder.DropTable(
            name: "SchedulerJob",
            schema: "server");

        migrationBuilder.DropColumn(
            name: "SchedulerStartTime",
            schema: "server",
            table: "SchedulerTask");

        migrationBuilder.DropColumn(
            name: "Version",
            schema: "server",
            table: "SchedulerResource");

        migrationBuilder.RenameColumn(
            name: "JobAppId",
            schema: "server",
            table: "SchedulerResource",
            newName: "ResourceType");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TaskRunStartTime",
            schema: "server",
            table: "SchedulerTask",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "TaskRunEndTime",
            schema: "server",
            table: "SchedulerTask",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DownloadUrl",
            schema: "server",
            table: "SchedulerResource",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "Job",
            schema: "server",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AlertMessageTemplate = table.Column<int>(type: "int", nullable: false),
                BelongProjectId = table.Column<int>(type: "int", nullable: false),
                BelongTeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                Creator = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                FailedRetryCount = table.Column<int>(type: "int", nullable: false),
                FailedRetryInterval = table.Column<int>(type: "int", nullable: false),
                FailedStrategy = table.Column<int>(type: "int", nullable: false),
                IsAlertException = table.Column<bool>(type: "bit", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                JobType = table.Column<int>(type: "int", nullable: false),
                MainFunc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                Modifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Principal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoutingStrategy = table.Column<int>(type: "int", nullable: false),
                RunTimeoutSecond = table.Column<int>(type: "int", nullable: false),
                RunTimeoutStrategy = table.Column<int>(type: "int", nullable: false),
                ScheduleBlockStrategy = table.Column<int>(type: "int", nullable: false),
                ScheduleExpiredStrategy = table.Column<int>(type: "int", nullable: false),
                ScheduleType = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Job", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "JobRunDetail",
            schema: "server",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FailedCount = table.Column<int>(type: "int", nullable: false),
                JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LastRunStatus = table.Column<int>(type: "int", nullable: false),
                LastRunTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                SuccessCount = table.Column<int>(type: "int", nullable: false),
                TimeoutCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobRunDetail", x => x.Id);
                table.ForeignKey(
                    name: "FK_JobRunDetail_Job_JobId",
                    column: x => x.JobId,
                    principalSchema: "server",
                    principalTable: "Job",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Job_BelongProjectId",
            schema: "server",
            table: "Job",
            column: "BelongProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Job_BelongTeamId",
            schema: "server",
            table: "Job",
            column: "BelongTeamId");

        migrationBuilder.CreateIndex(
            name: "IX_Job_Name",
            schema: "server",
            table: "Job",
            column: "Name",
            unique: true,
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_JobRunDetail_JobId",
            schema: "server",
            table: "JobRunDetail",
            column: "JobId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_SchedulerTask_Job_JobId",
            schema: "server",
            table: "SchedulerTask",
            column: "JobId",
            principalSchema: "server",
            principalTable: "Job",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
