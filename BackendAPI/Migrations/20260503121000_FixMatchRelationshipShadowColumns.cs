using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixMatchRelationshipShadowColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[FK_Matches_Clubs_ClubId]', N'F') IS NOT NULL
                    ALTER TABLE [Matches] DROP CONSTRAINT [FK_Matches_Clubs_ClubId];
                IF OBJECT_ID(N'[FK_Matches_Clubs_ClubId1]', N'F') IS NOT NULL
                    ALTER TABLE [Matches] DROP CONSTRAINT [FK_Matches_Clubs_ClubId1];
                IF OBJECT_ID(N'[FK_Matches_Stadiums_StadiumId1]', N'F') IS NOT NULL
                    ALTER TABLE [Matches] DROP CONSTRAINT [FK_Matches_Stadiums_StadiumId1];

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Matches_ClubId' AND object_id = OBJECT_ID(N'[Matches]'))
                    DROP INDEX [IX_Matches_ClubId] ON [Matches];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Matches_ClubId1' AND object_id = OBJECT_ID(N'[Matches]'))
                    DROP INDEX [IX_Matches_ClubId1] ON [Matches];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Matches_StadiumId1' AND object_id = OBJECT_ID(N'[Matches]'))
                    DROP INDEX [IX_Matches_StadiumId1] ON [Matches];

                IF COL_LENGTH(N'Matches', N'ClubId') IS NOT NULL
                    ALTER TABLE [Matches] DROP COLUMN [ClubId];
                IF COL_LENGTH(N'Matches', N'ClubId1') IS NOT NULL
                    ALTER TABLE [Matches] DROP COLUMN [ClubId1];
                IF COL_LENGTH(N'Matches', N'StadiumId1') IS NOT NULL
                    ALTER TABLE [Matches] DROP COLUMN [StadiumId1];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
