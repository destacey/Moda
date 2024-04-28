using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddindextoWorkItemsforsearching : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key_Title",
            schema: "Work",
            table: "WorkItems",
            columns: new[] { "Key", "Title" })
            .Annotation("SqlServer:Include", new[] { "Id", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Key_Title",
            schema: "Work",
            table: "WorkItems");
    }
}
