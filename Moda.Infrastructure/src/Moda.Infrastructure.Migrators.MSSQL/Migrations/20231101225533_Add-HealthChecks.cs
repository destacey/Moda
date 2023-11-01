using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddHealthChecks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Health");

        migrationBuilder.CreateTable(
            name: "HealthChecks",
            schema: "Health",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Context = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                ReportedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReportedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                Note = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
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
                table.PrimaryKey("PK_HealthChecks", x => x.Id);
                table.ForeignKey(
                    name: "FK_HealthChecks_Employees_ReportedById",
                    column: x => x.ReportedById,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_HealthChecks_Id",
            schema: "Health",
            table: "HealthChecks",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "ObjectId" });

        migrationBuilder.CreateIndex(
            name: "IX_HealthChecks_ObjectId",
            schema: "Health",
            table: "HealthChecks",
            column: "ObjectId")
            .Annotation("SqlServer:Include", new[] { "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_HealthChecks_ReportedById",
            schema: "Health",
            table: "HealthChecks",
            column: "ReportedById");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "HealthChecks",
            schema: "Health");
    }
}
