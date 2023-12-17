using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class UpdateAuditLogTableNameAfterPlanningIntervalRename : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // UPDATE TABLE NAME
        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [TableName] = 'PlanningInterval'
            WHERE [TableName] = 'ProgramIncrement'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [TableName] = 'PlanningIntervalObjective'
            WHERE [TableName] = 'ProgramIncrementObjective'");

        // UPDATE PRIMARY KEY NAME
        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [OldValues] = REPLACE(OldValues, '""programIncrementId"":', '""planningIntervalId"":')
            WHERE [OldValues] LIKE '%""programIncrementId"":%'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [NewValues] = REPLACE(NewValues, '""programIncrementId"":', '""planningIntervalId"":')
            WHERE [NewValues] LIKE '%""programIncrementId"":%'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [AffectedColumns] = REPLACE(AffectedColumns, 'ProgramIncrementId', 'PlanningIntervalId')
            WHERE [AffectedColumns] LIKE '%ProgramIncrementId%'");

        // UPDATE GOAL.OBJECTIVE TYPE
        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [OldValues] = REPLACE(OldValues, '""type"":""programIncrement""', '""type"":""planningInterval""')
            WHERE [TableName] = 'Objective' AND [OldValues] LIKE '%""type"":""programIncrement""%'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [NewValues] = REPLACE(NewValues, '""type"":""programIncrement""', '""type"":""planningInterval""')
            WHERE [TableName] = 'Objective' AND [NewValues] LIKE '%""type"":""programIncrement""%'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // UPDATE GOAL.OBJECTIVE TYPE
        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [OldValues] = REPLACE(OldValues, '""type"":""planningInterval""', '""type"":""programIncrement""')
            WHERE [TableName] = 'Objective' AND [OldValues] LIKE '%""type"":""planningInterval""%'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [NewValues] = REPLACE(NewValues, '""type"":""planningInterval""', '""type"":""programIncrement""')
            WHERE [TableName] = 'Objective' AND [NewValues] LIKE '%""type"":""planningInterval""%'");

        // UPDATE PRIMARY KEY NAME
        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [OldValues] = REPLACE(OldValues, '""planningIntervalId"":', '""programIncrementId"":')
            WHERE [OldValues] LIKE '%""planningIntervalId"":%'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [NewValues] = REPLACE(NewValues, '""planningIntervalId"":', '""programIncrementId"":')
            WHERE [NewValues] LIKE '%""planningIntervalId"":%'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [AffectedColumns] = REPLACE(AffectedColumns, 'PlanningIntervalId', 'ProgramIncrementId')
            WHERE [AffectedColumns] LIKE '%PlanningIntervalId%'");

        // UPDATE TABLE NAME
        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [TableName] = 'ProgramIncrement'
            WHERE [TableName] = 'PlanningInterval'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [TableName] = 'ProgramIncrementObjective'
            WHERE [TableName] = 'PlanningIntervalObjective'");
    }
}
