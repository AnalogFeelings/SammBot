using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBotNET.Migrations.PronounsDBMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pronouns",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    Object = table.Column<string>(type: "TEXT", nullable: true),
                    DependentPossessive = table.Column<string>(type: "TEXT", nullable: true),
                    IndependentPossessive = table.Column<string>(type: "TEXT", nullable: true),
                    ReflexiveSingular = table.Column<string>(type: "TEXT", nullable: true),
                    ReflexivePlural = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pronouns", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pronouns");
        }
    }
}
