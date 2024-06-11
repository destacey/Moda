using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddDoneTimestamptoWorkItem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_WorkItems_ExternalId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Id",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Key_Title",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.AddColumn<DateTime>(
            name: "DoneTimestamp",
            schema: "Work",
            table: "WorkItems",
            type: "datetime2",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ExternalId",
            schema: "Work",
            table: "WorkItems",
            column: "ExternalId")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "DoneTimestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Id",
            schema: "Work",
            table: "WorkItems",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "DoneTimestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "DoneTimestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key_Title",
            schema: "Work",
            table: "WorkItems",
            columns: new[] { "Key", "Title" })
            .Annotation("SqlServer:Include", new[] { "Id", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "DoneTimestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_StatusCategory",
            schema: "Work",
            table: "WorkItems",
            column: "StatusCategory")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId", "DoneTimestamp" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_WorkItems_ExternalId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Id",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_Key_Title",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_StatusCategory",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "DoneTimestamp",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ExternalId",
            schema: "Work",
            table: "WorkItems",
            column: "ExternalId")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Id",
            schema: "Work",
            table: "WorkItems",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key_Title",
            schema: "Work",
            table: "WorkItems",
            columns: new[] { "Key", "Title" })
            .Annotation("SqlServer:Include", new[] { "Id", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId" });
    }
}
