using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class AddSchedulerTaskIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId_TaskStatus",
                schema: "server",
                table: "SchedulerTask",
                columns: new[] { "JobId", "TaskStatus" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_JobId_TaskStatus",
                schema: "server",
                table: "SchedulerTask");
        }
    }
}
