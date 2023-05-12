using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddEmployeeFKstoRisk : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ReportedBy",
            schema: "Planning",
            table: "Risks",
            newName: "ReportedById");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_AssigneeId",
            schema: "Planning",
            table: "Risks",
            column: "AssigneeId");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_ReportedById",
            schema: "Planning",
            table: "Risks",
            column: "ReportedById");

        migrationBuilder.AddForeignKey(
            name: "FK_Risks_Employees_AssigneeId",
            schema: "Planning",
            table: "Risks",
            column: "AssigneeId",
            principalSchema: "Organization",
            principalTable: "Employees",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Risks_Employees_ReportedById",
            schema: "Planning",
            table: "Risks",
            column: "ReportedById",
            principalSchema: "Organization",
            principalTable: "Employees",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Risks_Employees_AssigneeId",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropForeignKey(
            name: "FK_Risks_Employees_ReportedById",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_Risks_AssigneeId",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_Risks_ReportedById",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.RenameColumn(
            name: "ReportedById",
            schema: "Planning",
            table: "Risks",
            newName: "ReportedBy");
    }
}
