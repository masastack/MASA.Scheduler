using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.EntityFrameworkCore.SqlServer.Migrations
{
    public partial class UpgradeSchedulerJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CronExpression",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CronExpression",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
