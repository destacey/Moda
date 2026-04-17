using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectBusinessCaseExpectedBenefits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Ppm",
                table: "Projects",
                type: "nvarchar(max)",
                maxLength: 4096,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCase",
                schema: "Ppm",
                table: "Projects",
                type: "nvarchar(max)",
                maxLength: 4096,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedBenefits",
                schema: "Ppm",
                table: "Projects",
                type: "nvarchar(max)",
                maxLength: 4096,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessCase",
                schema: "Ppm",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ExpectedBenefits",
                schema: "Ppm",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Ppm",
                table: "Projects",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 4096);
        }
    }
}
