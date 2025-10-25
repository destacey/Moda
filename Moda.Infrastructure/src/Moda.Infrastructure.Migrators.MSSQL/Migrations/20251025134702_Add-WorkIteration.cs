using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddWorkIteration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "IterationId",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "WorkIterations",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                End = table.Column<DateTime>(type: "datetime2", nullable: true),
                Start = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkIterations", x => x.Id);
                table.UniqueConstraint("AK_WorkIterations_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_WorkIterations_WorkTeams_TeamId",
                    column: x => x.TeamId,
                    principalSchema: "Work",
                    principalTable: "WorkTeams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_IterationId",
            schema: "Work",
            table: "WorkItems",
            column: "IterationId")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "WorkspaceId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp", "ProjectId", "ParentProjectId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkIterations_TeamId",
            schema: "Work",
            table: "WorkIterations",
            column: "TeamId");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkItems_WorkIterations_IterationId",
            schema: "Work",
            table: "WorkItems",
            column: "IterationId",
            principalSchema: "Work",
            principalTable: "WorkIterations",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkItems_WorkIterations_IterationId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropTable(
            name: "WorkIterations",
            schema: "Work");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_IterationId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropColumn(
            name: "IterationId",
            schema: "Work",
            table: "WorkItems");
    }
}
