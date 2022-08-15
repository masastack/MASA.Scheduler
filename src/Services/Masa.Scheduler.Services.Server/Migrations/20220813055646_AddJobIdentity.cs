using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class AddJobIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobIdentity",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobIdentity",
                schema: "server",
                table: "SchedulerJob");
        }
    }
}
