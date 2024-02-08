using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class Convertownedtypestocomplexproperties : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamMemberships_Start_End",
            schema: "Organization",
            table: "TeamMemberships");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervals_Start_End",
            schema: "Planning",
            table: "PlanningIntervals");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Start_End",
            schema: "Planning",
            table: "PlanningIntervalIterations");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberships_Start_End",
            schema: "Organization",
            table: "TeamMemberships",
            columns: new[] { "Start", "End" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervals_Start_End",
            schema: "Planning",
            table: "PlanningIntervals",
            columns: new[] { "Start", "End" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Start_End",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Start", "End" });
    }
}
