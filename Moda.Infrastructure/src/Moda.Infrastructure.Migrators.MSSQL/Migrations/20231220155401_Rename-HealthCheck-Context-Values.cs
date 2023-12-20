using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RenameHealthCheckContextValues : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE [Health].[HealthChecks]
            SET [Context] = 'PlanningPlanningIntervalObjective'
            WHERE [Context] = 'PlanningProgramIncrementObjective'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE [Health].[HealthChecks]
            SET [Context] = 'PlanningProgramIncrementObjective'
            WHERE [Context] = 'PlanningPlanningIntervalObjective'");
    }
}
