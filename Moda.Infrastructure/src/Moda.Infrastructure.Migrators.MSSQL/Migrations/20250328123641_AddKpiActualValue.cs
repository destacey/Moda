using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddKpiActualValue : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<double>(
            name: "ActualValue",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "float",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ActualValue",
            schema: "Ppm",
            table: "StrategicInitiativeKpis");
    }
}
