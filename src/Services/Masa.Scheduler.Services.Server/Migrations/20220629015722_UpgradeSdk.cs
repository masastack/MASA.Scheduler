using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class UpgradeSdk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerJob_BelongProjectId",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.DropColumn(
                name: "JobAppId",
                schema: "server",
                table: "SchedulerResource");

            migrationBuilder.DropColumn(
                name: "BelongProjectId",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.AddColumn<string>(
                name: "JobAppIdentity",
                schema: "server",
                table: "SchedulerResource",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BelongProjectIdentity",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_BelongProjectIdentity",
                schema: "server",
                table: "SchedulerJob",
                column: "BelongProjectIdentity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerJob_BelongProjectIdentity",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.DropColumn(
                name: "JobAppIdentity",
                schema: "server",
                table: "SchedulerResource");

            migrationBuilder.DropColumn(
                name: "BelongProjectIdentity",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.AddColumn<int>(
                name: "JobAppId",
                schema: "server",
                table: "SchedulerResource",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BelongProjectId",
                schema: "server",
                table: "SchedulerJob",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_BelongProjectId",
                schema: "server",
                table: "SchedulerJob",
                column: "BelongProjectId");
        }
    }
}
