using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masa.Scheduler.Services.Server.Migrations
{
    public partial class SchedulerJobTableUpgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertMessageTemplate",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.RenameColumn(
                name: "Principal",
                schema: "server",
                table: "SchedulerJob",
                newName: "Owner");

            migrationBuilder.RenameColumn(
                name: "MainFunc",
                schema: "server",
                table: "SchedulerJob",
                newName: "Origin");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TaskRunStartTime",
                schema: "server",
                table: "SchedulerTask",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TaskRunEndTime",
                schema: "server",
                table: "SchedulerTask",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DaprServiceInvocationConfig",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                schema: "server",
                table: "SchedulerJob",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HttpConfig",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobAppConfig",
                schema: "server",
                table: "SchedulerJob",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaprServiceInvocationConfig",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.DropColumn(
                name: "Enabled",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.DropColumn(
                name: "HttpConfig",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.DropColumn(
                name: "JobAppConfig",
                schema: "server",
                table: "SchedulerJob");

            migrationBuilder.RenameColumn(
                name: "Owner",
                schema: "server",
                table: "SchedulerJob",
                newName: "Principal");

            migrationBuilder.RenameColumn(
                name: "Origin",
                schema: "server",
                table: "SchedulerJob",
                newName: "MainFunc");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TaskRunStartTime",
                schema: "server",
                table: "SchedulerTask",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TaskRunEndTime",
                schema: "server",
                table: "SchedulerTask",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<int>(
                name: "AlertMessageTemplate",
                schema: "server",
                table: "SchedulerJob",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ResourceId",
                schema: "server",
                table: "SchedulerJob",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "server",
                table: "SchedulerJob",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
