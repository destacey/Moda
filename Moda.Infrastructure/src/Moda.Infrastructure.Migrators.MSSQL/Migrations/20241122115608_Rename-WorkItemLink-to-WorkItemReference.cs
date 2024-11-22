using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RenameWorkItemLinktoWorkItemReference : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rename the table instead of dropping and recreating
        migrationBuilder.RenameTable(
            name: "WorkItemLinks",
            schema: "Work",
            newName: "WorkItemReferences",
            newSchema: "Work");

        // Update indexes to use new table name
        migrationBuilder.RenameIndex(
            name: "IX_WorkItemLinks_WorkItemId",
            schema: "Work",
            table: "WorkItemReferences",
            newName: "IX_WorkItemReferences_WorkItemId");

        migrationBuilder.RenameIndex(
            name: "IX_WorkItemLinks_ObjectId_Context",
            schema: "Work",
            table: "WorkItemReferences",
            newName: "IX_WorkItemReferences_ObjectId_Context");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(
            name: "WorkItemReferences",
            schema: "Work",
            newName: "WorkItemLinks",
            newSchema: "Work");

        migrationBuilder.RenameIndex(
            name: "IX_WorkItemReferences_ObjectId_Context",
            schema: "Work",
            table: "WorkItemLinks",
            newName: "IX_WorkItemLinks_ObjectId_Context");

        migrationBuilder.RenameIndex(
            name: "IX_WorkItemReferences_WorkItemId",
            schema: "Work",
            table: "WorkItemLinks",
            newName: "IX_WorkItemLinks_WorkItemId");
    }
}
