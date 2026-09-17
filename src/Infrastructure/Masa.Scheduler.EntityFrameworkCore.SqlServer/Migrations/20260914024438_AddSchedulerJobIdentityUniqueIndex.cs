using Masa.Scheduler.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.EntityFrameworkCore.SqlServer.Migrations
{
    [DbContext(typeof(SchedulerDbContext))]
    [Migration("20260914024438_AddSchedulerJobIdentityUniqueIndex")]
    public partial class AddSchedulerJobIdentityUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE UNIQUE INDEX [IX_SchedulerJob_BelongProjectIdentity_JobIdentity]
ON [server].[SchedulerJob] ([BelongProjectIdentity], [JobIdentity])
WHERE [IsDeleted] = 0 AND [JobIdentity] IS NOT NULL AND [JobIdentity] <> '';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchedulerJob_BelongProjectIdentity_JobIdentity",
                schema: "server",
                table: "SchedulerJob");
        }
    }
}
