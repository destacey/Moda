using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AbilityToCreateUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "LoginProvider",
            schema: "Identity",
            table: "Users",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "");

        // Ensure all existing rows have the correct value
        migrationBuilder.Sql("UPDATE [Identity].[Users] SET [LoginProvider] = 'MicrosoftEntraId' WHERE [LoginProvider] = '' OR [LoginProvider] IS NULL;");

        migrationBuilder.AddColumn<string>(
            name: "RefreshToken",
            schema: "Identity",
            table: "Users",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "RefreshTokenExpiryTime",
            schema: "Identity",
            table: "Users",
            type: "datetime2",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LoginProvider",
            schema: "Identity",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "RefreshToken",
            schema: "Identity",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "RefreshTokenExpiryTime",
            schema: "Identity",
            table: "Users");
    }
}
