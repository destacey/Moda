using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class ChangeConnectionConfigurationSetup : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ConfigurationString",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "Configuration");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "AppIntegrations",
            table: "Connections",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(256)",
            oldMaxLength: 256);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Configuration",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "ConfigurationString");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "AppIntegrations",
            table: "Connections",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(128)",
            oldMaxLength: 128);
    }
}
