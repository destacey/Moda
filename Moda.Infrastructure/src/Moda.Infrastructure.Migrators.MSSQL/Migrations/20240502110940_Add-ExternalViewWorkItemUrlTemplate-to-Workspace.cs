using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddExternalViewWorkItemUrlTemplatetoWorkspace : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ExternalViewWorkItemUrlTemplate",
            schema: "Work",
            table: "Workspaces",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ExternalViewWorkItemUrlTemplate",
            schema: "Work",
            table: "Workspaces");
    }
}
