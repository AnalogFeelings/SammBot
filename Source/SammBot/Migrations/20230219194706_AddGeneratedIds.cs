using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBot.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pronouns",
                table: "Pronouns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildConfigs",
                table: "GuildConfigs");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserWarnings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserTags",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Pronouns",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Pronouns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pronouns",
                table: "Pronouns",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildConfigs",
                table: "GuildConfigs",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pronouns",
                table: "Pronouns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildConfigs",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Pronouns");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GuildConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "UserWarnings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "UserTags",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<ulong>(
                name: "UserId",
                table: "Pronouns",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pronouns",
                table: "Pronouns",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildConfigs",
                table: "GuildConfigs",
                column: "GuildId");
        }
    }
}
