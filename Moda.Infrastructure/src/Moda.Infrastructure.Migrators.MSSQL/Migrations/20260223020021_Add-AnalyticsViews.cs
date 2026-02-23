using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddAnalyticsViews : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Analytics");

        migrationBuilder.CreateTable(
            name: "AnalyticsViews",
            schema: "Analytics",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                Dataset = table.Column<int>(type: "int", nullable: false),
                DefinitionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Visibility = table.Column<int>(type: "int", nullable: false),
                OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AnalyticsViews", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AnalyticsViews_Name",
            schema: "Analytics",
            table: "AnalyticsViews",
            column: "Name")
            .Annotation("SqlServer:Include", new[] { "Dataset", "Visibility", "OwnerId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_AnalyticsViews_OwnerId",
            schema: "Analytics",
            table: "AnalyticsViews",
            column: "OwnerId")
            .Annotation("SqlServer:Include", new[] { "Name", "Dataset", "Visibility", "IsActive" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AnalyticsViews",
            schema: "Analytics");
    }
}
