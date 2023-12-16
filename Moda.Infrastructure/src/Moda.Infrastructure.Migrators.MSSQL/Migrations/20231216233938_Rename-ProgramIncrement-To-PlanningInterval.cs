using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class RenameProgramIncrementToPlanningInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningHealthChecks_ProgramIncrementObjectives_ObjectId",
                schema: "Planning",
                table: "PlanningHealthChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramIncrementObjectives_PlanningTeams_TeamId",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramIncrementObjectives_ProgramIncrements_PlanningIntervalId",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramIncrementTeams_PlanningTeams_TeamId",
                schema: "Planning",
                table: "ProgramIncrementTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramIncrementTeams_ProgramIncrements_PlanningIntervalId",
                schema: "Planning",
                table: "ProgramIncrementTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramIncrementTeams",
                schema: "Planning",
                table: "ProgramIncrementTeams");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProgramIncrements_Key",
                schema: "Planning",
                table: "ProgramIncrements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramIncrements",
                schema: "Planning",
                table: "ProgramIncrements");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProgramIncrementObjectives_Key",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramIncrementObjectives",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.RenameTable(
                name: "ProgramIncrementTeams",
                schema: "Planning",
                newName: "PlanningIntervalTeams",
                newSchema: "Planning");

            migrationBuilder.RenameTable(
                name: "ProgramIncrements",
                schema: "Planning",
                newName: "PlanningIntervals",
                newSchema: "Planning");

            migrationBuilder.RenameTable(
                name: "ProgramIncrementObjectives",
                schema: "Planning",
                newName: "PlanningIntervalObjectives",
                newSchema: "Planning");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementTeams_TeamId",
                schema: "Planning",
                table: "PlanningIntervalTeams",
                newName: "IX_PlanningIntervalTeams_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementTeams_PlanningIntervalId",
                schema: "Planning",
                table: "PlanningIntervalTeams",
                newName: "IX_PlanningIntervalTeams_PlanningIntervalId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrements_Start_End",
                schema: "Planning",
                table: "PlanningIntervals",
                newName: "IX_PlanningIntervals_Start_End");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrements_Name",
                schema: "Planning",
                table: "PlanningIntervals",
                newName: "IX_PlanningIntervals_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrements_IsDeleted",
                schema: "Planning",
                table: "PlanningIntervals",
                newName: "IX_PlanningIntervals_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrements_Id",
                schema: "Planning",
                table: "PlanningIntervals",
                newName: "IX_PlanningIntervals_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementObjectives_TeamId",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                newName: "IX_PlanningIntervalObjectives_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementObjectives_PlanningIntervalId_IsDeleted",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                newName: "IX_PlanningIntervalObjectives_PlanningIntervalId_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                newName: "IX_PlanningIntervalObjectives_ObjectiveId_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementObjectives_Key_IsDeleted",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                newName: "IX_PlanningIntervalObjectives_Key_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementObjectives_IsDeleted",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                newName: "IX_PlanningIntervalObjectives_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementObjectives_Id_IsDeleted",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                newName: "IX_PlanningIntervalObjectives_Id_IsDeleted");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanningIntervalTeams",
                schema: "Planning",
                table: "PlanningIntervalTeams",
                columns: new[] { "PlanningIntervalId", "TeamId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PlanningIntervals_Key",
                schema: "Planning",
                table: "PlanningIntervals",
                column: "Key");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanningIntervals",
                schema: "Planning",
                table: "PlanningIntervals",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PlanningIntervalObjectives_Key",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                column: "Key");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanningIntervalObjectives",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningHealthChecks_PlanningIntervalObjectives_ObjectId",
                schema: "Planning",
                table: "PlanningHealthChecks",
                column: "ObjectId",
                principalSchema: "Planning",
                principalTable: "PlanningIntervalObjectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningIntervalObjectives_PlanningIntervals_PlanningIntervalId",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                column: "PlanningIntervalId",
                principalSchema: "Planning",
                principalTable: "PlanningIntervals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningIntervalObjectives_PlanningTeams_TeamId",
                schema: "Planning",
                table: "PlanningIntervalObjectives",
                column: "TeamId",
                principalSchema: "Planning",
                principalTable: "PlanningTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningIntervalTeams_PlanningIntervals_PlanningIntervalId",
                schema: "Planning",
                table: "PlanningIntervalTeams",
                column: "PlanningIntervalId",
                principalSchema: "Planning",
                principalTable: "PlanningIntervals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningIntervalTeams_PlanningTeams_TeamId",
                schema: "Planning",
                table: "PlanningIntervalTeams",
                column: "TeamId",
                principalSchema: "Planning",
                principalTable: "PlanningTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanningHealthChecks_PlanningIntervalObjectives_ObjectId",
                schema: "Planning",
                table: "PlanningHealthChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningIntervalObjectives_PlanningIntervals_PlanningIntervalId",
                schema: "Planning",
                table: "PlanningIntervalObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningIntervalObjectives_PlanningTeams_TeamId",
                schema: "Planning",
                table: "PlanningIntervalObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningIntervalTeams_PlanningIntervals_PlanningIntervalId",
                schema: "Planning",
                table: "PlanningIntervalTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningIntervalTeams_PlanningTeams_TeamId",
                schema: "Planning",
                table: "PlanningIntervalTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanningIntervalTeams",
                schema: "Planning",
                table: "PlanningIntervalTeams");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PlanningIntervals_Key",
                schema: "Planning",
                table: "PlanningIntervals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanningIntervals",
                schema: "Planning",
                table: "PlanningIntervals");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PlanningIntervalObjectives_Key",
                schema: "Planning",
                table: "PlanningIntervalObjectives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanningIntervalObjectives",
                schema: "Planning",
                table: "PlanningIntervalObjectives");

            migrationBuilder.RenameTable(
                name: "PlanningIntervalTeams",
                schema: "Planning",
                newName: "ProgramIncrementTeams",
                newSchema: "Planning");

            migrationBuilder.RenameTable(
                name: "PlanningIntervals",
                schema: "Planning",
                newName: "ProgramIncrements",
                newSchema: "Planning");

            migrationBuilder.RenameTable(
                name: "PlanningIntervalObjectives",
                schema: "Planning",
                newName: "ProgramIncrementObjectives",
                newSchema: "Planning");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervalTeams_TeamId",
                schema: "Planning",
                table: "ProgramIncrementTeams",
                newName: "IX_ProgramIncrementTeams_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervalTeams_PlanningIntervalId",
                schema: "Planning",
                table: "ProgramIncrementTeams",
                newName: "IX_ProgramIncrementTeams_PlanningIntervalId");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervals_Start_End",
                schema: "Planning",
                table: "ProgramIncrements",
                newName: "IX_ProgramIncrements_Start_End");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervals_Name",
                schema: "Planning",
                table: "ProgramIncrements",
                newName: "IX_ProgramIncrements_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervals_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrements",
                newName: "IX_ProgramIncrements_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervals_Id",
                schema: "Planning",
                table: "ProgramIncrements",
                newName: "IX_ProgramIncrements_Id");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervalObjectives_TeamId",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "IX_ProgramIncrementObjectives_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervalObjectives_PlanningIntervalId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "IX_ProgramIncrementObjectives_PlanningIntervalId_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervalObjectives_ObjectiveId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervalObjectives_Key_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "IX_ProgramIncrementObjectives_Key_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervalObjectives_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "IX_ProgramIncrementObjectives_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningIntervalObjectives_Id_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "IX_ProgramIncrementObjectives_Id_IsDeleted");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramIncrementTeams",
                schema: "Planning",
                table: "ProgramIncrementTeams",
                columns: new[] { "PlanningIntervalId", "TeamId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProgramIncrements_Key",
                schema: "Planning",
                table: "ProgramIncrements",
                column: "Key");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramIncrements",
                schema: "Planning",
                table: "ProgramIncrements",
                column: "Id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProgramIncrementObjectives_Key",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                column: "Key");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramIncrementObjectives",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningHealthChecks_ProgramIncrementObjectives_ObjectId",
                schema: "Planning",
                table: "PlanningHealthChecks",
                column: "ObjectId",
                principalSchema: "Planning",
                principalTable: "ProgramIncrementObjectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramIncrementObjectives_PlanningTeams_TeamId",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                column: "TeamId",
                principalSchema: "Planning",
                principalTable: "PlanningTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_ProgramIncrementTeams_PlanningTeams_TeamId",
                schema: "Planning",
                table: "ProgramIncrementTeams",
                column: "TeamId",
                principalSchema: "Planning",
                principalTable: "PlanningTeams",
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
    }
}
