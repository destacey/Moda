using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddProgramIncrementTeams : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProgramIncrementTeams",
            schema: "Planning",
            columns: table => new
            {
                ProgramIncrementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProgramIncrementTeams", x => new { x.ProgramIncrementId, x.TeamId });
                table.ForeignKey(
                    name: "FK_ProgramIncrementTeams_ProgramIncrements_ProgramIncrementId",
                    column: x => x.ProgramIncrementId,
                    principalSchema: "Planning",
                    principalTable: "ProgramIncrements",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrementTeams_ProgramIncrementId",
            schema: "Planning",
            table: "ProgramIncrementTeams",
            column: "ProgramIncrementId")
            .Annotation("SqlServer:Include", new[] { "TeamId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProgramIncrementTeams",
            schema: "Planning");
    }
}
