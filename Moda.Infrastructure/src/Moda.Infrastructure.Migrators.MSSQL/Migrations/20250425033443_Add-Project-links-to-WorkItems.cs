using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddProjectlinkstoWorkItems : Migration
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
            name: "IX_WorkItems_StatusCategory",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.AddColumn<Guid>(
            name: "ParentProjectId",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ProjectId",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "WorkProjects",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkProjects", x => x.Id);
                table.UniqueConstraint("AK_WorkProjects_Key", x => x.Key);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ExternalId",
            schema: "Work",
            table: "WorkItems",
            column: "ExternalId")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp", "ProjectId", "ParentProjectId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Id",
            schema: "Work",
            table: "WorkItems",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp", "ProjectId", "ParentProjectId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp", "ProjectId", "ParentProjectId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ParentProjectId",
            schema: "Work",
            table: "WorkItems",
            column: "ParentProjectId",
            filter: "[ProjectId] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ProjectId_ParentProjectId",
            schema: "Work",
            table: "WorkItems",
            columns: new[] { "ProjectId", "ParentProjectId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_StatusCategory",
            schema: "Work",
            table: "WorkItems",
            column: "StatusCategory")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId", "ActivatedTimestamp", "DoneTimestamp", "ProjectId", "ParentProjectId" });

        migrationBuilder.AddForeignKey(
            name: "FK_WorkItems_WorkProjects_ParentProjectId",
            schema: "Work",
            table: "WorkItems",
            column: "ParentProjectId",
            principalSchema: "Work",
            principalTable: "WorkProjects",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkItems_WorkProjects_ProjectId",
            schema: "Work",
            table: "WorkItems",
            column: "ProjectId",
            principalSchema: "Work",
            principalTable: "WorkProjects",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkItems_WorkProjects_ParentProjectId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropForeignKey(
            name: "FK_WorkItems_WorkProjects_ProjectId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropTable(
            name: "WorkProjects",
            schema: "Work");

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
            name: "IX_WorkItems_ParentProjectId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_ProjectId_ParentProjectId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_StatusCategory",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "ParentProjectId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "ProjectId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ExternalId",
            schema: "Work",
            table: "WorkItems",
            column: "ExternalId")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Id",
            schema: "Work",
            table: "WorkItems",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_StatusCategory",
            schema: "Work",
            table: "WorkItems",
            column: "StatusCategory")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId", "ActivatedTimestamp", "DoneTimestamp" });
    }
}
