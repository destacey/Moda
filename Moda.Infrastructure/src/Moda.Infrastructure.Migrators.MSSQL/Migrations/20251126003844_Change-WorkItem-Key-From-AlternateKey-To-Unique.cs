using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class ChangeWorkItemKeyFromAlternateKeyToUnique : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropUniqueConstraint(
            name: "AK_WorkItems_Key",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems",
            column: "Key",
            unique: true)
            .Annotation("SqlServer:Include", new[] { "Id", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp", "ProjectId", "ParentProjectId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.AddUniqueConstraint(
            name: "AK_WorkItems_Key",
            schema: "Work",
            table: "WorkItems",
            column: "Key");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp", "ProjectId", "ParentProjectId" });
    }
}
