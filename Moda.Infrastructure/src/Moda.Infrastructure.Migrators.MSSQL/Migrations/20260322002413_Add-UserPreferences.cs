using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddUserPreferences : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Preferences",
            schema: "Identity",
            table: "Users",
            type: "nvarchar(max)",
            nullable: true);

        // Backfill existing users with empty preferences.
        // Tours is stored as a JSON string value due to the EF value conversion
        // (Dictionary<string,bool> -> string), so the format is {"Tours":"{}"}
        migrationBuilder.Sql(
            """
            UPDATE [Identity].[Users]
            SET [Preferences] = '{"Tours":"{}"}'
            WHERE [Preferences] IS NULL
        """);

        migrationBuilder.AlterColumn<string>(
            name: "Preferences",
            schema: "Identity",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Preferences",
            schema: "Identity",
            table: "Users");
    }
}
