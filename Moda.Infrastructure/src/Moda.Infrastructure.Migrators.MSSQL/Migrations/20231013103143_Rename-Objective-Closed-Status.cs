using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RenameObjectiveClosedStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE [Goals].[Objectives]
            SET [Status] = 'Completed'
            WHERE [Status] = 'Closed'");

        migrationBuilder.Sql(@"
            UPDATE [Planning].[ProgramIncrementObjectives]
            SET [Status] = 'Completed'
            WHERE [Status] = 'Closed'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE [Goals].[Objectives]
            SET [Status] = 'Closed'
            WHERE [Status] = 'Completed'");

        migrationBuilder.Sql(@"
            UPDATE [Planning].[ProgramIncrementObjectives]
            SET [Status] = 'Closed'
            WHERE [Status] = 'Completed'");
    }
}
