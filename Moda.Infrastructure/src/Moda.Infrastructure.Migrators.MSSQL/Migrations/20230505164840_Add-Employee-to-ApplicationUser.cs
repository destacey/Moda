using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddEmployeetoApplicationUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Id",
            schema: "Identity",
            table: "Users");

        migrationBuilder.AddColumn<Guid>(
            name: "EmployeeId",
            schema: "Identity",
            table: "Users",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_EmployeeId",
            schema: "Identity",
            table: "Users",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Id",
            schema: "Identity",
            table: "Users",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "UserName", "EmployeeId", "Email", "FirstName", "LastName" });

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Employees_EmployeeId",
            schema: "Identity",
            table: "Users",
            column: "EmployeeId",
            principalSchema: "Organization",
            principalTable: "Employees",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_Employees_EmployeeId",
            schema: "Identity",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_EmployeeId",
            schema: "Identity",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_Id",
            schema: "Identity",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "EmployeeId",
            schema: "Identity",
            table: "Users");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Id",
            schema: "Identity",
            table: "Users",
            column: "Id");
    }
}
