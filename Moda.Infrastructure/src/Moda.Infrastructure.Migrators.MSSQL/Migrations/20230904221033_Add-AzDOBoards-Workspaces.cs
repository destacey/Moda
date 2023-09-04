using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddAzDOBoardsWorkspaces : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AzdoBoardsWorkspaceConfigurations",
            schema: "AppIntegrations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                ConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Import = table.Column<bool>(type: "bit", nullable: false),
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
                table.PrimaryKey("PK_AzdoBoardsWorkspaceConfigurations", x => x.Id);
                table.ForeignKey(
                    name: "FK_AzdoBoardsWorkspaceConfigurations_Connections_ConnectionId",
                    column: x => x.ConnectionId,
                    principalSchema: "AppIntegrations",
                    principalTable: "Connections",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AzdoBoardsWorkspaceConfigurations_ConnectionId",
            schema: "AppIntegrations",
            table: "AzdoBoardsWorkspaceConfigurations",
            column: "ConnectionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AzdoBoardsWorkspaceConfigurations",
            schema: "AppIntegrations");
    }
}
