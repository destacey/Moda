using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddSystemAuditabletoWorkItems : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "SystemCreated",
            schema: "Work",
            table: "WorkItems",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "SystemLastModified",
            schema: "Work",
            table: "WorkItems",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SystemCreated",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "SystemLastModified",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkItems");
    }
}
