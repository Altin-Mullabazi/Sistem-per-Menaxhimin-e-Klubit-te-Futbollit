using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Matches_MatchDate_Status",
                table: "Matches",
                columns: new[] { "MatchDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate",
                table: "Transfers",
                column: "TransferDate");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_EndDate_Status",
                table: "Contracts",
                columns: new[] { "EndDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_Status_InjuryDate",
                table: "Injuries",
                columns: new[] { "Status", "InjuryDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matches_MatchDate_Status",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_EndDate_Status",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Injuries_Status_InjuryDate",
                table: "Injuries");
        }
    }
}
