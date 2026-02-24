using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class ReplaceKpiUnitwithPrefixandSuffix : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Prefix",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "nvarchar(8)",
            maxLength: 8,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Suffix",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "nvarchar(8)",
            maxLength: 8,
            nullable: true);

        // Migrate existing Unit data to Prefix/Suffix before dropping the column
        migrationBuilder.Sql("UPDATE [Ppm].[StrategicInitiativeKpis] SET [Prefix] = '$' WHERE [Unit] = 'USD'");
        migrationBuilder.Sql("UPDATE [Ppm].[StrategicInitiativeKpis] SET [Suffix] = '%' WHERE [Unit] = 'Percentage'");

        migrationBuilder.DropColumn(
            name: "Unit",
            schema: "Ppm",
            table: "StrategicInitiativeKpis");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Prefix",
            schema: "Ppm",
            table: "StrategicInitiativeKpis");

        migrationBuilder.DropColumn(
            name: "Suffix",
            schema: "Ppm",
            table: "StrategicInitiativeKpis");

        migrationBuilder.AddColumn<string>(
            name: "Unit",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "");
    }
}
