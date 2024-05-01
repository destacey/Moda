using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddParentIdtoWorkItems : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkItems_Employees_AssignedToId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.AddColumn<Guid>(
            name: "ParentId",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_ParentId",
            schema: "Work",
            table: "WorkItems",
            column: "ParentId");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkItems_Employees_AssignedToId",
            schema: "Work",
            table: "WorkItems",
            column: "AssignedToId",
            principalSchema: "Organization",
            principalTable: "Employees",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkItems_WorkItems_ParentId",
            schema: "Work",
            table: "WorkItems",
            column: "ParentId",
            principalSchema: "Work",
            principalTable: "WorkItems",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkItems_Employees_AssignedToId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropForeignKey(
            name: "FK_WorkItems_WorkItems_ParentId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_ParentId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "ParentId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkItems_Employees_AssignedToId",
            schema: "Work",
            table: "WorkItems",
            column: "AssignedToId",
            principalSchema: "Organization",
            principalTable: "Employees",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
