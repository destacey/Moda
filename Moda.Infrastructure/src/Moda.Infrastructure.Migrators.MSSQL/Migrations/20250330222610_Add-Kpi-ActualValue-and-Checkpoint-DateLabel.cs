using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddKpiActualValueandCheckpointDateLabel : Migration
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

        migrationBuilder.AddColumn<string>(
            name: "DateLabel",
            schema: "Ppm",
            table: "StrategicInitiativeKpiCheckpoints",
            type: "nvarchar(16)",
            maxLength: 16,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ActualValue",
            schema: "Ppm",
            table: "StrategicInitiativeKpis");

        migrationBuilder.DropColumn(
            name: "DateLabel",
            schema: "Ppm",
            table: "StrategicInitiativeKpiCheckpoints");
    }
}
