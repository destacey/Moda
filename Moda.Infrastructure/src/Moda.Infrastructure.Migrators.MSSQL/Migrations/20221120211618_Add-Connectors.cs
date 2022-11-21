using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AppIntegrations");

            migrationBuilder.CreateTable(
                name: "Connectors",
                schema: "AppIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
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
                    table.PrimaryKey("PK_Connectors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                schema: "Identity",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Id",
                schema: "Organization",
                table: "Employees",
                column: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Connectors",
                schema: "AppIntegrations");

            migrationBuilder.DropIndex(
                name: "IX_Users_Id",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Id",
                schema: "Organization",
                table: "Employees");
        }
    }
}
