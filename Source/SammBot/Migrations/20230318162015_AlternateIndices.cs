using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SammBot.Migrations
{
    /// <inheritdoc />
    public partial class AlternateIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Pronouns_UserId",
                table: "Pronouns",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuildConfigs_GuildId",
                table: "GuildConfigs",
                column: "GuildId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pronouns_UserId",
                table: "Pronouns");

            migrationBuilder.DropIndex(
                name: "IX_GuildConfigs_GuildId",
                table: "GuildConfigs");
        }
    }
}
