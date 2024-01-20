using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class ChangeAuditTrailCorrelationIdToString : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "CorrelationId",
            schema: "Auditing",
            table: "AuditTrails",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "CorrelationId",
            schema: "Auditing",
            table: "AuditTrails",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(128)",
            oldMaxLength: 128,
            oldNullable: true);
    }
}
