using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class FixAuditTrailEntries : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // fix the audit trail entries that were marked as Delete instead of Update
        migrationBuilder.Sql(
        @"
            UPDATE [Auditing].[AuditTrails]
            SET Type = 'Update'
            WHERE [Type] = 'Delete' AND NewValues NOT LIKE '%""isDeleted"":true%';
        ");

        // fix the audit trail entries that were marked as Updated instead of SoftDelete
        migrationBuilder.Sql(
        @"
            UPDATE [Auditing].[AuditTrails]
            SET Type = 'SoftDelete'
            WHERE [Type] = 'Update' AND NewValues LIKE '%""isDeleted"":true%';
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
