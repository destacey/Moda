using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamOperatingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamOperatingModels",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Methodology = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    SizingMethod = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    End = table.Column<DateTime>(type: "date", nullable: true),
                    Start = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamOperatingModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamOperatingModels_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "Organization",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamOperatingModels_TeamId",
                schema: "Organization",
                table: "TeamOperatingModels",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamOperatingModels_TeamId_Current",
                schema: "Organization",
                table: "TeamOperatingModels",
                column: "TeamId",
                filter: "[End] IS NULL")
                .Annotation("SqlServer:Include", new[] { "Id", "Methodology", "SizingMethod" });

            // Data migration: Create operating model records for existing teams
            migrationBuilder.Sql(@"
                INSERT INTO [Organization].[TeamOperatingModels]
                    (Id, TeamId, Start, [End], Methodology, SizingMethod, SystemCreated, SystemLastModified)
                SELECT
                    NEWID(),
                    t.Id,
                    t.ActiveDate,
                    NULL,
                    'Scrum',
                    'StoryPoints',
                    GETUTCDATE(),
                    GETUTCDATE()
                FROM [Organization].[Teams] t
                WHERE t.IsDeleted = 0
                    AND t.Type = 'Team'
                    AND NOT EXISTS (
                        SELECT 1 FROM [Organization].[TeamOperatingModels] m
                        WHERE m.TeamId = t.Id
                    );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamOperatingModels",
                schema: "Organization");
        }
    }
}
