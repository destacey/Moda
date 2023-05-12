using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class LinkProgramIncrementTeamtoPlanningTeam : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementTeams_TeamId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            column: "TeamId");

        migrationBuilder.AddForeignKey(
            name: "FK_ProgramIncrementTeams_PlanningTeams_TeamId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            column: "TeamId",
            principalSchema: "Planning",
            principalTable: "PlanningTeams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ProgramIncrementTeams_PlanningTeams_TeamId",
            schema: "Planning",
            table: "ProgramIncrementTeams");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementTeams_TeamId",
            schema: "Planning",
            table: "ProgramIncrementTeams");
    }
}
