using Microsoft.EntityFrameworkCore.Migrations;

namespace SammBotNET.Migrations.PhrasesDBMigrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Phrase",
				columns: table => new
				{
					Content = table.Column<string>(type: "TEXT", nullable: false),
					AuthorId = table.Column<ulong>(type: "INTEGER", nullable: false),
					ServerId = table.Column<ulong>(type: "INTEGER", nullable: false),
					CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Phrase", x => x.Content);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Phrase");
		}
	}
}
