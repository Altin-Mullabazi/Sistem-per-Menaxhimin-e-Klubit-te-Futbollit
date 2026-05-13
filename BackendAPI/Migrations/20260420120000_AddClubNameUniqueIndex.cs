using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddClubNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Clubs_Name",
                table: "Clubs",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clubs_Name",
                table: "Clubs");
        }
    }
}
