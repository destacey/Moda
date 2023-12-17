using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class UpdateGoalsObjectivesTypeAfterPlanningIntervalRename : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE [Goals].[Objectives]
            SET [Type] = 'PlanningInterval'
            WHERE [Type] = 'ProgramIncrement'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE [Goals].[Objectives]
            SET [Type] = 'ProgramIncrement'
            WHERE [Type] = 'PlanningInterval'");
    }
}
