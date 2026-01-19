using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddPlanningIntervalIterationSprint : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PlanningIntervalIterationSprints",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PlanningIntervalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PlanningIntervalIterationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SprintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlanningIntervalIterationSprints", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlanningIntervalIterationSprints_Iterations_SprintId",
                    column: x => x.SprintId,
                    principalSchema: "Planning",
                    principalTable: "Iterations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlanningIntervalIterationSprints_PlanningIntervalIterations_PlanningIntervalIterationId",
                    column: x => x.PlanningIntervalIterationId,
                    principalSchema: "Planning",
                    principalTable: "PlanningIntervalIterations",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_PlanningIntervalIterationSprints_PlanningIntervals_PlanningIntervalId",
                    column: x => x.PlanningIntervalId,
                    principalSchema: "Planning",
                    principalTable: "PlanningIntervals",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterationSprints_PlanningIntervalId",
            schema: "Planning",
            table: "PlanningIntervalIterationSprints",
            column: "PlanningIntervalId")
            .Annotation("SqlServer:Include", new[] { "PlanningIntervalIterationId", "SprintId" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterationSprints_PlanningIntervalIterationId",
            schema: "Planning",
            table: "PlanningIntervalIterationSprints",
            column: "PlanningIntervalIterationId")
            .Annotation("SqlServer:Include", new[] { "PlanningIntervalId", "SprintId" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterationSprints_SprintId",
            schema: "Planning",
            table: "PlanningIntervalIterationSprints",
            column: "SprintId",
            unique: true)
            .Annotation("SqlServer:Include", new[] { "PlanningIntervalId", "PlanningIntervalIterationId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PlanningIntervalIterationSprints",
            schema: "Planning");
    }
}
