using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddWorkItems : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WorkItems",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExternalId = table.Column<int>(type: "int", nullable: true),
                TypeId = table.Column<int>(type: "int", nullable: false),
                StatusId = table.Column<int>(type: "int", nullable: false),
                AssignedToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Priority = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkItems", x => x.Id);
                table.UniqueConstraint("AK_WorkItems_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_WorkItems_Employees_AssignedToId",
                    column: x => x.AssignedToId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItems_Employees_CreatedById",
                    column: x => x.CreatedById,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItems_Employees_LastModifiedById",
                    column: x => x.LastModifiedById,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItems_WorkStatuses_StatusId",
                    column: x => x.StatusId,
                    principalSchema: "Work",
                    principalTable: "WorkStatuses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItems_WorkTypes_TypeId",
                    column: x => x.TypeId,
                    principalSchema: "Work",
                    principalTable: "WorkTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItems_Workspaces_WorkspaceId",
                    column: x => x.WorkspaceId,
                    principalSchema: "Work",
                    principalTable: "Workspaces",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_AssignedToId",
            schema: "Work",
            table: "WorkItems",
            column: "AssignedToId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_CreatedById",
            schema: "Work",
            table: "WorkItems",
            column: "CreatedById");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ExternalId",
            schema: "Work",
            table: "WorkItems",
            column: "ExternalId")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId", "Priority" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Id",
            schema: "Work",
            table: "WorkItems",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "Priority" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_Key",
            schema: "Work",
            table: "WorkItems",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Title", "WorkspaceId", "ExternalId", "AssignedToId", "TypeId", "StatusId", "Priority" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_LastModifiedById",
            schema: "Work",
            table: "WorkItems",
            column: "LastModifiedById");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_StatusId",
            schema: "Work",
            table: "WorkItems",
            column: "StatusId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_TypeId",
            schema: "Work",
            table: "WorkItems",
            column: "TypeId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_WorkspaceId",
            schema: "Work",
            table: "WorkItems",
            column: "WorkspaceId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkItems",
            schema: "Work");
    }
}
