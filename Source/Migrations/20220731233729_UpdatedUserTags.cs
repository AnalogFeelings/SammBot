using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBotNET.Migrations
{
	public partial class UpdatedUserTags : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "ServerId",
				table: "UserTags",
				newName: "GuildId");

			migrationBuilder.AlterColumn<string>(
				name: "Id",
				table: "UserTags",
				type: "TEXT",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "INTEGER")
				.OldAnnotation("Sqlite:Autoincrement", true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "GuildId",
				table: "UserTags",
				newName: "ServerId");

			migrationBuilder.AlterColumn<int>(
				name: "Id",
				table: "UserTags",
				type: "INTEGER",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "TEXT")
				.Annotation("Sqlite:Autoincrement", true);
		}
	}
}
