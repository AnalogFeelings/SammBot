using Microsoft.EntityFrameworkCore.Migrations;

namespace SammBotNET.Migrations.PeoneImagesDBMigrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "PeoneImage",
				columns: table => new
				{
					TwitterUrl = table.Column<string>(type: "TEXT", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PeoneImage", x => x.TwitterUrl);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PeoneImage");
		}
	}
}
