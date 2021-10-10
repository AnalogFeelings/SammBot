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
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    authorID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    serverID = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phrase", x => x.content);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Phrase");
        }
    }
}
