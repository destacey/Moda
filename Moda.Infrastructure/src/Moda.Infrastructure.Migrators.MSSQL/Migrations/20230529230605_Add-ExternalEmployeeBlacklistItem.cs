using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddExternalEmployeeBlacklistItem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExternalEmployeeBlacklistItems",
            schema: "Organization",
            columns: table => new
            {
                ObjectId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExternalEmployeeBlacklistItems", x => x.ObjectId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ExternalEmployeeBlacklistItems_ObjectId",
            schema: "Organization",
            table: "ExternalEmployeeBlacklistItems",
            column: "ObjectId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ExternalEmployeeBlacklistItems",
            schema: "Organization");
    }
}
