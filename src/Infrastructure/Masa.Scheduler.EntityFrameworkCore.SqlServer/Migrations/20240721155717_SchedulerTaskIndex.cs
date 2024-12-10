using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.EntityFrameworkCore.SqlServer.Migrations
{
    public partial class SchedulerTaskIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_JobId_TaskStatus",
                schema: "server",
                table: "SchedulerTask");

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
            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_JobId_IsDeleted_TaskStatus",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_Origin_IsDeleted",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId_TaskStatus",
                schema: "server",
                table: "SchedulerTask",
                columns: new[] { "JobId", "TaskStatus" });
        }
    }
}
