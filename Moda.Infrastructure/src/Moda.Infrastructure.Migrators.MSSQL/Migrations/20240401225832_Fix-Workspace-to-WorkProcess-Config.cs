using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class FixWorkspacetoWorkProcessConfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Workspaces_WorkProcesses_WorkProcessId",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.AddForeignKey(
            name: "FK_Workspaces_WorkProcesses_WorkProcessId",
            schema: "Work",
            table: "Workspaces",
            column: "WorkProcessId",
            principalSchema: "Work",
            principalTable: "WorkProcesses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Workspaces_WorkProcesses_WorkProcessId",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.AddForeignKey(
            name: "FK_Workspaces_WorkProcesses_WorkProcessId",
            schema: "Work",
            table: "Workspaces",
            column: "WorkProcessId",
            principalSchema: "Work",
            principalTable: "WorkProcesses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
