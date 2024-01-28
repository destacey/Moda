using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddWorkProcessSchemeAndWorkFlow : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Workflows",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                Ownership = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Workflows", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "WorkflowSchemes",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WorkStatusId = table.Column<int>(type: "int", nullable: false),
                WorkStatusCategory = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkflowSchemes", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkflowSchemes_WorkStatuses_WorkStatusId",
                    column: x => x.WorkStatusId,
                    principalSchema: "Work",
                    principalTable: "WorkStatuses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkflowSchemes_Workflows_WorkflowId",
                    column: x => x.WorkflowId,
                    principalSchema: "Work",
                    principalTable: "Workflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WorkProcessSchemes",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WorkProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WorkTypeId = table.Column<int>(type: "int", nullable: false),
                WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkProcessSchemes", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkProcessSchemes_WorkProcesses_WorkProcessId",
                    column: x => x.WorkProcessId,
                    principalSchema: "Work",
                    principalTable: "WorkProcesses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_WorkProcessSchemes_WorkTypes_WorkTypeId",
                    column: x => x.WorkTypeId,
                    principalSchema: "Work",
                    principalTable: "WorkTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkProcessSchemes_Workflows_WorkProcessId",
                    column: x => x.WorkProcessId,
                    principalSchema: "Work",
                    principalTable: "Workflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_Id",
            schema: "Work",
            table: "Workflows",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_IsActive_IsDeleted",
            schema: "Work",
            table: "Workflows",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_Id",
            schema: "Work",
            table: "WorkflowSchemes",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkflowSchemes",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_WorkflowId",
            schema: "Work",
            table: "WorkflowSchemes",
            column: "WorkflowId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_WorkStatusId",
            schema: "Work",
            table: "WorkflowSchemes",
            column: "WorkStatusId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcessSchemes_Id",
            schema: "Work",
            table: "WorkProcessSchemes",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcessSchemes_WorkProcessId",
            schema: "Work",
            table: "WorkProcessSchemes",
            column: "WorkProcessId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcessSchemes_WorkTypeId",
            schema: "Work",
            table: "WorkProcessSchemes",
            column: "WorkTypeId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkflowSchemes",
            schema: "Work");

        migrationBuilder.DropTable(
            name: "WorkProcessSchemes",
            schema: "Work");

        migrationBuilder.DropTable(
            name: "Workflows",
            schema: "Work");
    }
}
