using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class FixWorkIterationtoWorkTeammapping : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkIterations_WorkTeams_TeamId",
            schema: "Work",
            table: "WorkIterations");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkIterations_WorkTeams_TeamId",
            schema: "Work",
            table: "WorkIterations",
            column: "TeamId",
            principalSchema: "Work",
            principalTable: "WorkTeams",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkIterations_WorkTeams_TeamId",
            schema: "Work",
            table: "WorkIterations");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkIterations_WorkTeams_TeamId",
            schema: "Work",
            table: "WorkIterations",
            column: "TeamId",
            principalSchema: "Work",
            principalTable: "WorkTeams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
