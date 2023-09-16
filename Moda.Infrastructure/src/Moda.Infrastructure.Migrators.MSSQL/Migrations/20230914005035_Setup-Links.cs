using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class SetupLinks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Links");

        migrationBuilder.CreateTable(
            name: "Links",
            schema: "Links",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Links", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Links_Id",
            schema: "Links",
            table: "Links",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "ObjectId", "Name", "Url" });

        migrationBuilder.CreateIndex(
            name: "IX_Links_ObjectId",
            schema: "Links",
            table: "Links",
            column: "ObjectId")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Url" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Links",
            schema: "Links");
    }
}
