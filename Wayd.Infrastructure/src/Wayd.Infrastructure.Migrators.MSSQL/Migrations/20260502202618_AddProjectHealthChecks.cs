using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddProjectHealthChecks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProjectHealthChecks",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                ReportedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReportedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                Note = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectHealthChecks", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProjectHealthChecks_Employees_ReportedById",
                    column: x => x.ReportedById,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProjectHealthChecks_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalSchema: "Ppm",
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProjectHealthChecks_ProjectId_Expiration_IsDeleted",
            schema: "Ppm",
            table: "ProjectHealthChecks",
            columns: new[] { "ProjectId", "Expiration", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectHealthChecks_ReportedById",
            schema: "Ppm",
            table: "ProjectHealthChecks",
            column: "ReportedById");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProjectHealthChecks",
            schema: "Ppm");
    }
}
