using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class SetWorkProcessWorkflowIdasoptional : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkProcessSchemes_Workflows_WorkProcessId",
            schema: "Work",
            table: "WorkProcessSchemes");

        migrationBuilder.AlterColumn<Guid>(
            name: "WorkflowId",
            schema: "Work",
            table: "WorkProcessSchemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcessSchemes_WorkflowId",
            schema: "Work",
            table: "WorkProcessSchemes",
            column: "WorkflowId");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkProcessSchemes_Workflows_WorkflowId",
            schema: "Work",
            table: "WorkProcessSchemes",
            column: "WorkflowId",
            principalSchema: "Work",
            principalTable: "Workflows",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkProcessSchemes_Workflows_WorkflowId",
            schema: "Work",
            table: "WorkProcessSchemes");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcessSchemes_WorkflowId",
            schema: "Work",
            table: "WorkProcessSchemes");

        migrationBuilder.AlterColumn<Guid>(
            name: "WorkflowId",
            schema: "Work",
            table: "WorkProcessSchemes",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_WorkProcessSchemes_Workflows_WorkProcessId",
            schema: "Work",
            table: "WorkProcessSchemes",
            column: "WorkProcessId",
            principalSchema: "Work",
            principalTable: "Workflows",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
