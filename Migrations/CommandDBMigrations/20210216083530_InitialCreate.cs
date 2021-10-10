using Microsoft.EntityFrameworkCore.Migrations;

namespace SammBotNET.Migrations.CommandDBMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomCommand",
                columns: table => new
                {
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    authorID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    reply = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCommand", x => x.name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomCommand");
        }
    }
}
