using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class RenameLocalIdtoKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Teams_LocalId",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Code",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Id",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_IsDeleted",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Risks_LocalId",
                schema: "Planning",
                table: "Risks");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProgramIncrements_LocalId",
                schema: "Planning",
                table: "ProgramIncrements");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProgramIncrementObjectives_LocalId",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIncrementObjectives_Id_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIncrementObjectives_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIncrementObjectives_ProgramIncrementId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PlanningTeams_LocalId",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlanningTeams_Code",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlanningTeams_Id_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlanningTeams_IsActive_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlanningTeams_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Objectives_LocalId",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_Id_IsDeleted",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_IsDeleted",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_OwnerId_IsDeleted",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_PlanId_IsDeleted",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Employees_LocalId",
                schema: "Organization",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "LocalId",
                schema: "Organization",
                table: "Teams",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_LocalId",
                schema: "Organization",
                table: "Teams",
                newName: "IX_Teams_Key");

            migrationBuilder.RenameColumn(
                name: "LocalId",
                schema: "Planning",
                table: "Risks",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "LocalId",
                schema: "Planning",
                table: "ProgramIncrements",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "LocalId",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementObjectives_LocalId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "IX_ProgramIncrementObjectives_Key_IsDeleted");

            migrationBuilder.RenameColumn(
                name: "LocalId",
                schema: "Planning",
                table: "PlanningTeams",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningTeams_LocalId_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams",
                newName: "IX_PlanningTeams_Key_IsDeleted");

            migrationBuilder.RenameColumn(
                name: "LocalId",
                schema: "Goals",
                table: "Objectives",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_Objectives_LocalId_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                newName: "IX_Objectives_Key_IsDeleted");

            migrationBuilder.RenameColumn(
                name: "LocalId",
                schema: "Organization",
                table: "Employees",
                newName: "Key");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Teams_Key",
                schema: "Organization",
                table: "Teams",
                column: "Key");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Risks_Key",
                schema: "Planning",
                table: "Risks",
                column: "Key");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProgramIncrements_Key",
                schema: "Planning",
                table: "ProgramIncrements",
                column: "Key");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProgramIncrementObjectives_Key",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                column: "Key");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PlanningTeams_Key",
                schema: "Planning",
                table: "PlanningTeams",
                column: "Key");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Objectives_Key",
                schema: "Goals",
                table: "Objectives",
                column: "Key");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Employees_Key",
                schema: "Organization",
                table: "Employees",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Code",
                schema: "Organization",
                table: "Teams",
                column: "Code",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Id",
                schema: "Organization",
                table: "Teams",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_IsDeleted",
                schema: "Organization",
                table: "Teams",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type", "IsActive" });

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
                name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                columns: new[] { "ObjectiveId", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "ProgramIncrementId", "Type", "IsStretch" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramIncrementObjectives_ProgramIncrementId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                columns: new[] { "ProgramIncrementId", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "ObjectiveId", "Type", "IsStretch" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningTeams_Code",
                schema: "Planning",
                table: "PlanningTeams",
                column: "Code",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningTeams_Id_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams",
                columns: new[] { "Id", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningTeams_IsActive_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams",
                columns: new[] { "IsActive", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningTeams_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_Id_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                columns: new[] { "Id", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Key", "Name", "Type", "Status", "OwnerId", "PlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId", "PlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_OwnerId_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                columns: new[] { "OwnerId", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "PlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_PlanId_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                columns: new[] { "PlanId", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Teams_Key",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Code",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Id",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_IsDeleted",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Risks_Key",
                schema: "Planning",
                table: "Risks");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProgramIncrements_Key",
                schema: "Planning",
                table: "ProgramIncrements");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProgramIncrementObjectives_Key",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIncrementObjectives_Id_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIncrementObjectives_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ProgramIncrementObjectives_ProgramIncrementId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PlanningTeams_Key",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlanningTeams_Code",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlanningTeams_Id_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlanningTeams_IsActive_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropIndex(
                name: "IX_PlanningTeams_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Objectives_Key",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_Id_IsDeleted",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_IsDeleted",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_OwnerId_IsDeleted",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_PlanId_IsDeleted",
                schema: "Goals",
                table: "Objectives");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Employees_Key",
                schema: "Organization",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "Organization",
                table: "Teams",
                newName: "LocalId");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_Key",
                schema: "Organization",
                table: "Teams",
                newName: "IX_Teams_LocalId");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "Planning",
                table: "Risks",
                newName: "LocalId");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "Planning",
                table: "ProgramIncrements",
                newName: "LocalId");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "LocalId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramIncrementObjectives_Key_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                newName: "IX_ProgramIncrementObjectives_LocalId_IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "Planning",
                table: "PlanningTeams",
                newName: "LocalId");

            migrationBuilder.RenameIndex(
                name: "IX_PlanningTeams_Key_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams",
                newName: "IX_PlanningTeams_LocalId_IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "Goals",
                table: "Objectives",
                newName: "LocalId");

            migrationBuilder.RenameIndex(
                name: "IX_Objectives_Key_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                newName: "IX_Objectives_LocalId_IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "Organization",
                table: "Employees",
                newName: "LocalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Teams_LocalId",
                schema: "Organization",
                table: "Teams",
                column: "LocalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Risks_LocalId",
                schema: "Planning",
                table: "Risks",
                column: "LocalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProgramIncrements_LocalId",
                schema: "Planning",
                table: "ProgramIncrements",
                column: "LocalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProgramIncrementObjectives_LocalId",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                column: "LocalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PlanningTeams_LocalId",
                schema: "Planning",
                table: "PlanningTeams",
                column: "LocalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Objectives_LocalId",
                schema: "Goals",
                table: "Objectives",
                column: "LocalId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Employees_LocalId",
                schema: "Organization",
                table: "Employees",
                column: "LocalId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Code",
                schema: "Organization",
                table: "Teams",
                column: "Code",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Id",
                schema: "Organization",
                table: "Teams",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "LocalId", "Name", "Code", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_IsDeleted",
                schema: "Organization",
                table: "Teams",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "Code", "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramIncrementObjectives_Id_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                columns: new[] { "Id", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "LocalId", "ProgramIncrementId", "ObjectiveId", "Type", "IsStretch" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramIncrementObjectives_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "ProgramIncrementId", "ObjectiveId", "Type", "IsStretch" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramIncrementObjectives_ObjectiveId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                columns: new[] { "ObjectiveId", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "ProgramIncrementId", "Type", "IsStretch" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramIncrementObjectives_ProgramIncrementId_IsDeleted",
                schema: "Planning",
                table: "ProgramIncrementObjectives",
                columns: new[] { "ProgramIncrementId", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "ObjectiveId", "Type", "IsStretch" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningTeams_Code",
                schema: "Planning",
                table: "PlanningTeams",
                column: "Code",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningTeams_Id_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams",
                columns: new[] { "Id", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "LocalId", "Name", "Code", "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningTeams_IsActive_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams",
                columns: new[] { "IsActive", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "Code", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningTeams_IsDeleted",
                schema: "Planning",
                table: "PlanningTeams",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "Code", "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_Id_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                columns: new[] { "Id", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "LocalId", "Name", "Type", "Status", "OwnerId", "PlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "Type", "Status", "OwnerId", "PlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_OwnerId_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                columns: new[] { "OwnerId", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "Type", "Status", "PlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_PlanId_IsDeleted",
                schema: "Goals",
                table: "Objectives",
                columns: new[] { "PlanId", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "Type", "Status", "OwnerId" });
        }
    }
}
