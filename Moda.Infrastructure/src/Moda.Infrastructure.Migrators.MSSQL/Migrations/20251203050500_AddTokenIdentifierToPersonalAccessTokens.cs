using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenIdentifierToPersonalAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenIdentifier",
                schema: "Identity",
                table: "PersonalAccessTokens",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccessTokens_TokenIdentifier_RevokedAt_ExpiresAt",
                schema: "Identity",
                table: "PersonalAccessTokens",
                columns: new[] { "TokenIdentifier", "RevokedAt", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonalAccessTokens_TokenIdentifier_RevokedAt_ExpiresAt",
                schema: "Identity",
                table: "PersonalAccessTokens");

            migrationBuilder.DropColumn(
                name: "TokenIdentifier",
                schema: "Identity",
                table: "PersonalAccessTokens");
        }
    }
}
