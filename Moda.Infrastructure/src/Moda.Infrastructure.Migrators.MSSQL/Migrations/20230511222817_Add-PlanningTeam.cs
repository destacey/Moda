using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddPlanningTeam : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PlanningTeams",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LocalId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlanningTeams", x => x.Id);
                table.UniqueConstraint("AK_PlanningTeams_LocalId", x => x.LocalId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Risks_TeamId",
            schema: "Planning",
            table: "Risks",
            column: "TeamId");

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
            name: "IX_PlanningTeams_LocalId_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "LocalId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.AddForeignKey(
            name: "FK_Risks_PlanningTeams_TeamId",
            schema: "Planning",
            table: "Risks",
            column: "TeamId",
            principalSchema: "Planning",
            principalTable: "PlanningTeams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.Sql(@"
            INSERT INTO [Planning].[PlanningTeams]
            SELECT [Id]
                  ,[LocalId]
                  ,[Name]
                  ,[Code]
                  ,[Type]
                  ,[IsActive]
                  ,[Deleted]
                  ,[DeletedBy]
                  ,[IsDeleted]
              FROM [Organization].[Teams]");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Risks_PlanningTeams_TeamId",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropTable(
            name: "PlanningTeams",
            schema: "Planning");

        migrationBuilder.DropIndex(
            name: "IX_Risks_TeamId",
            schema: "Planning",
            table: "Risks");
    }
}
