using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddStrategicInitiativeKpiOrder : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Order",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "int",
            nullable: false,
            defaultValue: 0);

        // Backfill Order for existing KPIs: sequence per StrategicInitiative by Key ascending.
        migrationBuilder.Sql(@"
                WITH ordered AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (PARTITION BY StrategicInitiativeId ORDER BY [Key]) AS NewOrder
                    FROM Ppm.StrategicInitiativeKpis
                )
                UPDATE k
                SET k.[Order] = ordered.NewOrder
                FROM Ppm.StrategicInitiativeKpis k
                INNER JOIN ordered ON k.Id = ordered.Id;
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Order",
            schema: "Ppm",
            table: "StrategicInitiativeKpis");
    }
}
