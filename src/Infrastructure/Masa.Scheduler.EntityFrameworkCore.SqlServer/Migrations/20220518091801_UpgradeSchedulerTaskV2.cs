using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.EntityFrameworkCore.SqlServer.Migrations
{
    public partial class UpgradeSchedulerTaskV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.RenameColumn(
                name: "SchedulerStartTime",
                schema: "server",
                table: "SchedulerTask",
                newName: "SchedulerTime");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                schema: "server",
                table: "SchedulerTask",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RunUserId",
                schema: "server",
                table: "SchedulerTask",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SpecifiedWorkerHost",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask",
                column: "JobId",
                filter: "[IsDeleted] = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropColumn(
                name: "Message",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropColumn(
                name: "RunUserId",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropColumn(
                name: "SpecifiedWorkerHost",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.RenameColumn(
                name: "SchedulerTime",
                schema: "server",
                table: "SchedulerTask",
                newName: "SchedulerStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask",
                column: "JobId");
        }
    }
}
