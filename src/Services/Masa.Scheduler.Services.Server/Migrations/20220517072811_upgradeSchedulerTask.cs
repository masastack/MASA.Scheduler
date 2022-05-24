using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class upgradeSchedulerTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Origin",
                schema: "server",
                table: "SchedulerTask",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkerHost",
                schema: "server",
                table: "SchedulerTask",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropColumn(
                name: "WorkerHost",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropColumn(
                name: "CronExpression",
                schema: "server",
                table: "SchedulerJob");
        }
    }
}
