using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddPIObjectiveStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            type: "varchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AddColumn<string>(
            name: "Status",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            type: "varchar(64)",
            maxLength: 64,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql("UPDATE Planning.ProgramIncrementObjectives SET [Status] = goal.[Status] FROM Planning.ProgramIncrementObjectives pio INNER JOIN Goals.Objectives goal ON pio.ObjectiveId = goal.Id WHERE pio.[Status] IS NULL OR pio.[Status] = ''");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Status",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(64)",
            oldMaxLength: 64);
    }
}
