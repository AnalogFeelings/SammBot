using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBotNET.Migrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "UserTag",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					Name = table.Column<string>(type: "TEXT", nullable: true),
					Reply = table.Column<string>(type: "TEXT", nullable: true),
					AuthorId = table.Column<ulong>(type: "INTEGER", nullable: false),
					ServerId = table.Column<ulong>(type: "INTEGER", nullable: false),
					CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserTag", x => x.Id);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "UserTag");
		}
	}
}
