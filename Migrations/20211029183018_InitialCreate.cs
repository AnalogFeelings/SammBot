using Microsoft.EntityFrameworkCore.Migrations;

namespace SammBotNET.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomCommand",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Reply = table.Column<string>(type: "TEXT", nullable: true),
                    AuthorId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCommand", x => x.Name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomCommand");
        }
    }
}
