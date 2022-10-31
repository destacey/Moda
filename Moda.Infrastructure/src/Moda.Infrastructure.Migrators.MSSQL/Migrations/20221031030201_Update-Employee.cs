using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                schema: "Organization",
                table: "Employees",
                newName: "EmployeeNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_EmployeeId",
                schema: "Organization",
                table: "Employees",
                newName: "IX_Employees_EmployeeNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmployeeNumber",
                schema: "Organization",
                table: "Employees",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_EmployeeNumber",
                schema: "Organization",
                table: "Employees",
                newName: "IX_Employees_EmployeeId");
        }
    }
}
