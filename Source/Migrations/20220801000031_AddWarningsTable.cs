using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBotNET.Migrations
{
	public partial class AddWarningsTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "UserWarnings",
				columns: table => new
				{
					Id = table.Column<string>(type: "TEXT", nullable: false),
					UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
					GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
					Reason = table.Column<string>(type: "TEXT", nullable: true),
					Date = table.Column<long>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserWarnings", x => x.Id);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "UserWarnings");
		}
	}
}
