using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBot.Migrations
{
    public partial class GuildConfig_LoggingWelcome : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WarningLimitAction",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "WarningLimit",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "EnableLogging",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWelcome",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<ulong>(
                name: "LogChannel",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "WelcomeChannel",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeMessage",
                table: "GuildConfigs",
                type: "TEXT",
                nullable: true,
                defaultValue: "{0}, welcome to {1}! Remember to read the rules before chatting!");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableLogging",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "EnableWelcome",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "LogChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "WelcomeChannel",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "WelcomeMessage",
                table: "GuildConfigs");

            migrationBuilder.AlterColumn<int>(
                name: "WarningLimitAction",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 2);

            migrationBuilder.AlterColumn<int>(
                name: "WarningLimit",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 3);
        }
    }
}
