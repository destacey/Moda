using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddinitialStrategicManagementconfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "StrategicManagement");

        migrationBuilder.CreateTable(
            name: "StrategicThemes",
            schema: "StrategicManagement",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategicThemes", x => x.Id);
                table.UniqueConstraint("AK_StrategicThemes_Key", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "Strategies",
            schema: "StrategicManagement",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                Description = table.Column<string>(type: "nvarchar(3072)", maxLength: 3072, nullable: true),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Start = table.Column<DateTime>(type: "date", nullable: true),
                End = table.Column<DateTime>(type: "date", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Strategies", x => x.Id);
                table.UniqueConstraint("AK_Strategies_Key", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "Visions",
            schema: "StrategicManagement",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Description = table.Column<string>(type: "nvarchar(3072)", maxLength: 3072, nullable: false),
                State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Start = table.Column<DateTime>(type: "datetime2", nullable: true),
                End = table.Column<DateTime>(type: "datetime2", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Visions", x => x.Id);
                table.UniqueConstraint("AK_Visions_Key", x => x.Key);
            });

        migrationBuilder.CreateIndex(
            name: "IX_StrategicThemes_State",
            schema: "StrategicManagement",
            table: "StrategicThemes",
            column: "State");

        migrationBuilder.CreateIndex(
            name: "IX_Strategies_Status",
            schema: "StrategicManagement",
            table: "Strategies",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Visions_State",
            schema: "StrategicManagement",
            table: "Visions",
            column: "State");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "StrategicThemes",
            schema: "StrategicManagement");

        migrationBuilder.DropTable(
            name: "Strategies",
            schema: "StrategicManagement");

        migrationBuilder.DropTable(
            name: "Visions",
            schema: "StrategicManagement");
    }
}
