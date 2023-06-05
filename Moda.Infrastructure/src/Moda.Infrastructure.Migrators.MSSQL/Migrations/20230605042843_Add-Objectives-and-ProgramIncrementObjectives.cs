using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddObjectivesandProgramIncrementObjectives : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Goals");

        migrationBuilder.CreateTable(
            name: "Objectives",
            schema: "Goals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LocalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                StartDate = table.Column<DateTime>(type: "date", nullable: true),
                TargetDate = table.Column<DateTime>(type: "date", nullable: true),
                ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                table.PrimaryKey("PK_Objectives", x => x.Id);
                table.UniqueConstraint("AK_Objectives_LocalId", x => x.LocalId);
            });

        migrationBuilder.CreateTable(
            name: "ProgramIncrementObjectives",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LocalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProgramIncrementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ObjectiveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                IsStretch = table.Column<bool>(type: "bit", nullable: false),
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
                table.PrimaryKey("PK_ProgramIncrementObjectives", x => x.Id);
                table.UniqueConstraint("AK_ProgramIncrementObjectives_LocalId", x => x.LocalId);
                table.ForeignKey(
                    name: "FK_ProgramIncrementObjectives_PlanningTeams_TeamId",
                    column: x => x.TeamId,
                    principalSchema: "Planning",
                    principalTable: "PlanningTeams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProgramIncrementObjectives_ProgramIncrements_ProgramIncrementId",
                    column: x => x.ProgramIncrementId,
                    principalSchema: "Planning",
                    principalTable: "ProgramIncrements",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

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
            name: "IX_Objectives_LocalId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "LocalId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Type", "Status", "OwnerId", "PlanId" });

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
            name: "IX_ProgramIncrementObjectives_LocalId_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            columns: new[] { "LocalId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "ProgramIncrementId", "ObjectiveId", "Type", "IsStretch" });

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
            name: "IX_ProgramIncrementObjectives_TeamId",
            schema: "Planning",
            table: "ProgramIncrementObjectives",
            column: "TeamId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Objectives",
            schema: "Goals");

        migrationBuilder.DropTable(
            name: "ProgramIncrementObjectives",
            schema: "Planning");
    }
}
