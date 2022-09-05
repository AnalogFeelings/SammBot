using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBotNET.Migrations
{
    public partial class RemovePeoneTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeoneImages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeoneImages",
                columns: table => new
                {
                    TwitterUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeoneImages", x => x.TwitterUrl);
                });
        }
    }
}
