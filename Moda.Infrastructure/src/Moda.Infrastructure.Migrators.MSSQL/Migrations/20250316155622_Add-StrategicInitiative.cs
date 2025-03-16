using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddStrategicInitiative : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence(
            name: "StrategicInitiativeKpiSequence",
            schema: "Work");

        migrationBuilder.CreateTable(
            name: "StrategicInitiatives",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Start = table.Column<DateTime>(type: "date", nullable: false),
                End = table.Column<DateTime>(type: "date", nullable: false),
                PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategicInitiatives", x => x.Id);
                table.UniqueConstraint("AK_StrategicInitiatives_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_StrategicInitiatives_Portfolios_PortfolioId",
                    column: x => x.PortfolioId,
                    principalSchema: "Ppm",
                    principalTable: "Portfolios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StrategicInitiativeKpis",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [Work].[StrategicInitiativeKpiSequence]"),
                StrategicInitiativeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                TargetValue = table.Column<double>(type: "float", nullable: false),
                Unit = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                TargetDirection = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategicInitiativeKpis", x => x.Id);
                table.UniqueConstraint("AK_StrategicInitiativeKpis_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_StrategicInitiativeKpis_StrategicInitiatives_StrategicInitiativeId",
                    column: x => x.StrategicInitiativeId,
                    principalSchema: "Ppm",
                    principalTable: "StrategicInitiatives",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StrategicInitiativeProjects",
            schema: "Ppm",
            columns: table => new
            {
                StrategicInitiativeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategicInitiativeProjects", x => new { x.StrategicInitiativeId, x.ProjectId });
                table.ForeignKey(
                    name: "FK_StrategicInitiativeProjects_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalSchema: "Ppm",
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_StrategicInitiativeProjects_StrategicInitiatives_StrategicInitiativeId",
                    column: x => x.StrategicInitiativeId,
                    principalSchema: "Ppm",
                    principalTable: "StrategicInitiatives",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "StrategicInitiativeRoleAssignments",
            schema: "Ppm",
            columns: table => new
            {
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Role = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategicInitiativeRoleAssignments", x => new { x.ObjectId, x.EmployeeId, x.Role });
                table.ForeignKey(
                    name: "FK_StrategicInitiativeRoleAssignments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_StrategicInitiativeRoleAssignments_StrategicInitiatives_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Ppm",
                    principalTable: "StrategicInitiatives",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StrategicInitiativeKpiCheckpoints",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                KpiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TargetValue = table.Column<double>(type: "float", nullable: false),
                CheckpointDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategicInitiativeKpiCheckpoints", x => x.Id);
                table.ForeignKey(
                    name: "FK_StrategicInitiativeKpiCheckpoints_StrategicInitiativeKpis_KpiId",
                    column: x => x.KpiId,
                    principalSchema: "Ppm",
                    principalTable: "StrategicInitiativeKpis",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StrategicInitiativeKpiMeasurements",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                KpiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ActualValue = table.Column<double>(type: "float", nullable: false),
                MeasurementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                MeasuredById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Note = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategicInitiativeKpiMeasurements", x => x.Id);
                table.ForeignKey(
                    name: "FK_StrategicInitiativeKpiMeasurements_Employees_MeasuredById",
                    column: x => x.MeasuredById,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_StrategicInitiativeKpiMeasurements_StrategicInitiativeKpis_KpiId",
                    column: x => x.KpiId,
                    principalSchema: "Ppm",
                    principalTable: "StrategicInitiativeKpis",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiativeKpiCheckpoints_KpiId",
            schema: "Ppm",
            table: "StrategicInitiativeKpiCheckpoints",
            column: "KpiId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiativeKpiMeasurements_KpiId",
            schema: "Ppm",
            table: "StrategicInitiativeKpiMeasurements",
            column: "KpiId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiativeKpiMeasurements_MeasuredById",
            schema: "Ppm",
            table: "StrategicInitiativeKpiMeasurements",
            column: "MeasuredById");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiativeKpis_StrategicInitiativeId",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            column: "StrategicInitiativeId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiativeProjects_ProjectId",
            schema: "Ppm",
            table: "StrategicInitiativeProjects",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiativeProjects_StrategicInitiativeId",
            schema: "Ppm",
            table: "StrategicInitiativeProjects",
            column: "StrategicInitiativeId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiativeRoleAssignments_EmployeeId",
            schema: "Ppm",
            table: "StrategicInitiativeRoleAssignments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiativeRoleAssignments_ObjectId",
            schema: "Ppm",
            table: "StrategicInitiativeRoleAssignments",
            column: "ObjectId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiatives_PortfolioId",
            schema: "Ppm",
            table: "StrategicInitiatives",
            column: "PortfolioId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicInitiatives_Status",
            schema: "Ppm",
            table: "StrategicInitiatives",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "StrategicInitiativeKpiCheckpoints",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "StrategicInitiativeKpiMeasurements",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "StrategicInitiativeProjects",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "StrategicInitiativeRoleAssignments",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "StrategicInitiativeKpis",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "StrategicInitiatives",
            schema: "Ppm");

        migrationBuilder.DropSequence(
            name: "StrategicInitiativeKpiSequence",
            schema: "Work");
    }
}
