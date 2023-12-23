using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddPIIterationAndUpdateEnumDbConfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Organization",
            table: "Teams",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AlterColumn<string>(
            name: "Code",
            schema: "Organization",
            table: "Teams",
            type: "varchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "Planning",
            table: "Risks",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AlterColumn<string>(
            name: "Likelihood",
            schema: "Planning",
            table: "Risks",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AlterColumn<string>(
            name: "Impact",
            schema: "Planning",
            table: "Risks",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AlterColumn<string>(
            name: "Category",
            schema: "Planning",
            table: "Risks",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Planning",
            table: "PlanningTeams",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AlterColumn<string>(
            name: "Code",
            schema: "Planning",
            table: "PlanningTeams",
            type: "varchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(64)",
            oldMaxLength: 64);

        migrationBuilder.CreateTable(
            name: "PlanningIntervalIterations",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PlanningIntervalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Start = table.Column<DateTime>(type: "date", nullable: false),
                End = table.Column<DateTime>(type: "date", nullable: false),
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
                table.PrimaryKey("PK_PlanningIntervalIterations", x => x.Id);
                table.UniqueConstraint("AK_PlanningIntervalIterations_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_PlanningIntervalIterations_PlanningIntervals_PlanningIntervalId",
                    column: x => x.PlanningIntervalId,
                    principalSchema: "Planning",
                    principalTable: "PlanningIntervals",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "PlanningIntervalId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Start_End",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Start", "End" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PlanningIntervalIterations",
            schema: "Planning");

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Organization",
            table: "Teams",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);

        migrationBuilder.AlterColumn<string>(
            name: "Code",
            schema: "Organization",
            table: "Teams",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "Planning",
            table: "Risks",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);

        migrationBuilder.AlterColumn<string>(
            name: "Likelihood",
            schema: "Planning",
            table: "Risks",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);

        migrationBuilder.AlterColumn<string>(
            name: "Impact",
            schema: "Planning",
            table: "Risks",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);

        migrationBuilder.AlterColumn<string>(
            name: "Category",
            schema: "Planning",
            table: "Risks",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Planning",
            table: "PlanningTeams",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);

        migrationBuilder.AlterColumn<string>(
            name: "Code",
            schema: "Planning",
            table: "PlanningTeams",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "varchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "varchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);
    }
}
