using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBot.Bot.Migrations
{
    /// <inheritdoc />
    public partial class RemovePronouns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pronouns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pronouns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DependentPossessive = table.Column<string>(type: "TEXT", nullable: false),
                    IndependentPossessive = table.Column<string>(type: "TEXT", nullable: false),
                    Object = table.Column<string>(type: "TEXT", nullable: false),
                    ReflexivePlural = table.Column<string>(type: "TEXT", nullable: false),
                    ReflexiveSingular = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pronouns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pronouns_UserId",
                table: "Pronouns",
                column: "UserId",
                unique: true);
        }
    }
}
