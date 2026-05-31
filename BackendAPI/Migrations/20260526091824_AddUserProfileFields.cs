using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clubs_AspNetUsers_CreatedById",
                table: "Clubs");

            migrationBuilder.DropForeignKey(
                name: "FK_Clubs_LegacyUsers_UserId",
                table: "Clubs");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Seasons",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_UserId",
                table: "Seasons",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_AspNetUsers_CreatedById",
                table: "Clubs",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_LegacyUsers_UserId",
                table: "Clubs",
                column: "UserId",
                principalTable: "LegacyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Seasons_LegacyUsers_UserId",
                table: "Seasons",
                column: "UserId",
                principalTable: "LegacyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clubs_AspNetUsers_CreatedById",
                table: "Clubs");

            migrationBuilder.DropForeignKey(
                name: "FK_Clubs_LegacyUsers_UserId",
                table: "Clubs");

            migrationBuilder.DropForeignKey(
                name: "FK_Seasons_LegacyUsers_UserId",
                table: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Seasons_UserId",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_AspNetUsers_CreatedById",
                table: "Clubs",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_LegacyUsers_UserId",
                table: "Clubs",
                column: "UserId",
                principalTable: "LegacyUsers",
                principalColumn: "Id");
        }
    }
}
