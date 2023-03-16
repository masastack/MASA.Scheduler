using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class SchedulerJobNotifyUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "index_state_timessent_modificationtime",
                table: "IntegrationEventLog",
                newName: "IX_State_TimesSent_MTime");

            migrationBuilder.RenameIndex(
                name: "index_state_modificationtime",
                table: "IntegrationEventLog",
                newName: "IX_State_MTime");

            migrationBuilder.RenameIndex(
                name: "index_eventid_version",
                table: "IntegrationEventLog",
                newName: "IX_EventId_Version");

            migrationBuilder.AddColumn<string>(
                name: "NotifyUrl",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyUrl",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.RenameIndex(
                name: "IX_State_TimesSent_MTime",
                table: "IntegrationEventLog",
                newName: "index_state_timessent_modificationtime");

            migrationBuilder.RenameIndex(
                name: "IX_State_MTime",
                table: "IntegrationEventLog",
                newName: "index_state_modificationtime");

            migrationBuilder.RenameIndex(
                name: "IX_EventId_Version",
                table: "IntegrationEventLog",
                newName: "index_eventid_version");
        }
    }
}
