using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddWorkTeamandupdatePlanningTeamconfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_IsActive_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropColumn(
            name: "Deleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.AddColumn<Guid>(
            name: "TeamId",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "WorkTeams",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                Type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkTeams", x => x.Id);
                table.UniqueConstraint("AK_WorkTeams_Key", x => x.Key);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_TeamId",
            schema: "Work",
            table: "WorkItems",
            column: "TeamId");

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_Id",
            schema: "Planning",
            table: "PlanningTeams",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_IsActive",
            schema: "Planning",
            table: "PlanningTeams",
            column: "IsActive")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_Key",
            schema: "Planning",
            table: "PlanningTeams",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkTeams_Code",
            schema: "Work",
            table: "WorkTeams",
            column: "Code",
            unique: true)
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkTeams_Id",
            schema: "Work",
            table: "WorkTeams",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkTeams_IsActive",
            schema: "Work",
            table: "WorkTeams",
            column: "IsActive")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkTeams_Key",
            schema: "Work",
            table: "WorkTeams",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.AddForeignKey(
            name: "FK_WorkItems_WorkTeams_TeamId",
            schema: "Work",
            table: "WorkItems",
            column: "TeamId",
            principalSchema: "Work",
            principalTable: "WorkTeams",
            principalColumn: "Id");

        migrationBuilder.Sql(@"
            IF EXISTS (SELECT 1 FROM [Organization].[Teams])
            BEGIN
                INSERT INTO [Work].[WorkTeams]
                SELECT [Id]
                      ,[Key]
                      ,[Name]
                      ,[Code]
                      ,[Type]
                      ,[IsActive]
                  FROM [Organization].[Teams]
            END");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkItems_WorkTeams_TeamId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropTable(
            name: "WorkTeams",
            schema: "Work");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_TeamId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_Id",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_IsActive",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_Key",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropColumn(
            name: "TeamId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.AddColumn<DateTime>(
            name: "Deleted",
            schema: "Planning",
            table: "PlanningTeams",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "DeletedBy",
            schema: "Planning",
            table: "PlanningTeams",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_IsActive_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "Type", "IsActive" });
    }
}
