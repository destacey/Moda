using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RenameIterationTypetoIterationCategory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.RenameColumn(
            name: "Type",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "Category");

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "PlanningIntervalId", "Name", "Category" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "PlanningIntervalId", "Name", "Category" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "PlanningIntervalId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Category" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.RenameColumn(
            name: "Category",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "Type");

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "PlanningIntervalId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type" });
    }
}
