using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddOidcProviders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OidcProviders",
            schema: "Identity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ProviderType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                Authority = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                ClientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Audience = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Scopes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                AllowedTenantIds = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                ClockSkewSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OidcProviders", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "UX_OidcProviders_Name",
            schema: "Identity",
            table: "OidcProviders",
            column: "Name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OidcProviders",
            schema: "Identity");
    }
}
