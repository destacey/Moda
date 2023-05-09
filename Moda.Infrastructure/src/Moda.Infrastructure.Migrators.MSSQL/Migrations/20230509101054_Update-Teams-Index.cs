using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class UpdateTeamsIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Teams_IsDeleted",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_IsDeleted",
            schema: "Organization",
            table: "Teams",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "Code", "Type", "IsActive" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Teams_IsDeleted",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_IsDeleted",
            schema: "Organization",
            table: "Teams",
            column: "IsDeleted");
    }
}
