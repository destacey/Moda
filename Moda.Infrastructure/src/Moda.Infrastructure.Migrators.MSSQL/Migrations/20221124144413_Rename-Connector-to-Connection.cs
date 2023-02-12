using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class RenameConnectortoConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Connectors",
                schema: "AppIntegrations");

            migrationBuilder.CreateTable(
                name: "Connections",
                schema: "AppIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Connector = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ConfigurationString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsValidConfiguration = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_Connector_IsActive",
                schema: "AppIntegrations",
                table: "Connections",
                columns: new[] { "Connector", "IsActive" })
                .Annotation("SqlServer:Include", new[] { "Id", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Connections_Id",
                schema: "AppIntegrations",
                table: "Connections",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_IsActive",
                schema: "AppIntegrations",
                table: "Connections",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_IsDeleted",
                schema: "AppIntegrations",
                table: "Connections",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Connections",
                schema: "AppIntegrations");

            migrationBuilder.CreateTable(
                name: "Connectors",
                schema: "AppIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigurationString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsValidConfiguration = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connectors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_Id",
                schema: "AppIntegrations",
                table: "Connectors",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_IsActive",
                schema: "AppIntegrations",
                table: "Connectors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_IsDeleted",
                schema: "AppIntegrations",
                table: "Connectors",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_Type_IsActive",
                schema: "AppIntegrations",
                table: "Connectors",
                columns: new[] { "Type", "IsActive" })
                .Annotation("SqlServer:Include", new[] { "Id", "Name" });
        }
    }
}
