using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class RemoveNameUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropIndex(
                name: "IX_SchedulerResource_Name",
                schema: "server",
                table: "SchedulerResource");

            migrationBuilder.DropIndex(
                name: "IX_SchedulerJob_Name",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerResource_Name",
                schema: "server",
                table: "SchedulerResource",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_Name",
                schema: "server",
                table: "SchedulerJob",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask");

            migrationBuilder.DropIndex(
                name: "IX_SchedulerResource_Name",
                schema: "server",
                table: "SchedulerResource");

            migrationBuilder.DropIndex(
                name: "IX_SchedulerJob_Name",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerTask_JobId",
                schema: "server",
                table: "SchedulerTask",
                column: "JobId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerResource_Name",
                schema: "server",
                table: "SchedulerResource",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulerJob_Name",
                schema: "server",
                table: "SchedulerJob",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
