using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class UpdateEmployeeNumberUniqueIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Employees_EmployeeNumber",
            schema: "Organization",
            table: "Employees");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_EmployeeNumber",
            schema: "Organization",
            table: "Employees",
            column: "EmployeeNumber",
            unique: true,
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Employees_EmployeeNumber",
            schema: "Organization",
            table: "Employees");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_EmployeeNumber",
            schema: "Organization",
            table: "Employees",
            column: "EmployeeNumber",
            unique: true)
            .Annotation("SqlServer:Include", new[] { "Id" });
    }
}
