using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RenameColumnProgramIncrementIdToPlanningIntervalId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ProgramIncrementObjectives_ProgramIncrements_ProgramIncrementId",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.DropForeignKey(
            name: "FK_ProgramIncrementTeams_ProgramIncrements_ProgramIncrementId",
            schema: "Planning",
            table: "ProgramIncrementTeams");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementObjectives_Id_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementObjectives_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementObjectives_Key_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.RenameColumn(
            name: "ProgramIncrementId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            newName: "PlanningIntervalId");

        migrationBuilder.RenameIndex(
            name: "IX_ProgramIncrementTeams_ProgramIncrementId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            newName: "IX_ProgramIncrementTeams_PlanningIntervalId");

        migrationBuilder.RenameColumn(
            name: "ProgramIncrementId",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            newName: "PlanningIntervalId");

        migrationBuilder.RenameIndex(
            name: "IX_ProgramIncrementObjectives_ProgramIncrementId_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            newName: "IX_ProgramIncrementObjectives_PlanningIntervalId_IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementObjectives_Id_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "PlanningIntervalId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementObjectives_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "PlanningIntervalId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementObjectives_Key_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "PlanningIntervalId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            columns: new[] { "ObjectiveId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "PlanningIntervalId", "Type", "IsStretch" });

        migrationBuilder.AddForeignKey(
            name: "FK_ProgramIncrementObjectives_ProgramIncrements_PlanningIntervalId",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            column: "PlanningIntervalId",
            principalSchema: "Planning",
            principalTable: "ProgramIncrements",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ProgramIncrementTeams_ProgramIncrements_PlanningIntervalId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            column: "PlanningIntervalId",
            principalSchema: "Planning",
            principalTable: "ProgramIncrements",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ProgramIncrementObjectives_ProgramIncrements_PlanningIntervalId",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.DropForeignKey(
            name: "FK_ProgramIncrementTeams_ProgramIncrements_PlanningIntervalId",
            schema: "Planning",
            table: "ProgramIncrementTeams");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementObjectives_Id_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementObjectives_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementObjectives_Key_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.DropIndex(
            name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives");

        migrationBuilder.RenameColumn(
            name: "PlanningIntervalId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            newName: "ProgramIncrementId");

        migrationBuilder.RenameIndex(
            name: "IX_ProgramIncrementTeams_PlanningIntervalId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            newName: "IX_ProgramIncrementTeams_ProgramIncrementId");

        migrationBuilder.RenameColumn(
            name: "PlanningIntervalId",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            newName: "ProgramIncrementId");

        migrationBuilder.RenameIndex(
            name: "IX_ProgramIncrementObjectives_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            newName: "IX_ProgramIncrementObjectives_ProgramIncrementId_IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementObjectives_Id_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "ProgramIncrementId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementObjectives_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "ProgramIncrementId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementObjectives_Key_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "ProgramIncrementId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            columns: new[] { "ObjectiveId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "ProgramIncrementId", "Type", "IsStretch" });

        migrationBuilder.AddForeignKey(
            name: "FK_ProgramIncrementObjectives_ProgramIncrements_ProgramIncrementId",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            column: "ProgramIncrementId",
            principalSchema: "Planning",
            principalTable: "ProgramIncrements",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ProgramIncrementTeams_ProgramIncrements_ProgramIncrementId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            column: "ProgramIncrementId",
            principalSchema: "Planning",
            principalTable: "ProgramIncrements",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
