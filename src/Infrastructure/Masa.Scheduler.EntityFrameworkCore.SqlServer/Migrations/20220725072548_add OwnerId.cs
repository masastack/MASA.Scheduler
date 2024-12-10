using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.EntityFrameworkCore.SqlServer.Migrations
{
    public partial class addOwnerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                schema: "server",
                table: "SchedulerJob",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "server",
                table: "SchedulerJob");
        }
    }
}
