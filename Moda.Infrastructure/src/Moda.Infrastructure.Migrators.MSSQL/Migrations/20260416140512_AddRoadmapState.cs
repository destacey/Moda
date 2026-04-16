using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddRoadmapState : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "State",
            schema: "Planning",
            table: "Roadmaps",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "Active");

        migrationBuilder.Sql("UPDATE [Planning].[Roadmaps] SET [State] = 'Active' WHERE [State] = '' OR [State] IS NULL;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "State",
            schema: "Planning",
            table: "Roadmaps");
    }
}
