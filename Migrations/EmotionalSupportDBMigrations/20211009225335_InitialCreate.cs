using Microsoft.EntityFrameworkCore.Migrations;

namespace SammBotNET.Migrations.EmotionalSupportDBMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmotionalSupport",
                columns: table => new
                {
                    SupportMessage = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmotionalSupport", x => x.SupportMessage);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmotionalSupport");
        }
    }
}
