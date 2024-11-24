using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class Indeximprovements : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_ExternalId",
            schema: "Work",
            table: "Workspaces",
            column: "ExternalId",
            filter: "[ExternalId] IS NOT NULL")
            .Annotation("SqlServer:Include", new[] { "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ExternalId_WorkspaceId",
            schema: "Work",
            table: "WorkItems",
            columns: new[] { "ExternalId", "WorkspaceId" },
            filter: "[ExternalId] IS NOT NULL")
            .Annotation("SqlServer:Include", new[] { "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_Employees_Email",
            schema: "Organization",
            table: "Employees",
            column: "Email")
            .Annotation("SqlServer:Include", new[] { "Id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Workspaces_ExternalId",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_ExternalId_WorkspaceId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_Employees_Email",
            schema: "Organization",
            table: "Employees");
    }
}
