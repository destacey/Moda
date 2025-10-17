using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddIterations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Iterations",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                End = table.Column<DateTime>(type: "datetime2", nullable: true),
                Start = table.Column<DateTime>(type: "datetime2", nullable: true),
                ExternalId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                Ownership = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                SystemId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Iterations", x => x.Id);
                table.UniqueConstraint("AK_Iterations_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_Iterations_PlanningTeams_TeamId",
                    column: x => x.TeamId,
                    principalSchema: "Planning",
                    principalTable: "PlanningTeams",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "IterationExternalMetadata",
            schema: "Planning",
            columns: table => new
            {
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IterationExternalMetadata", x => new { x.ObjectId, x.Name });
                table.ForeignKey(
                    name: "FK_IterationExternalMetadata_Iterations_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Planning",
                    principalTable: "Iterations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_IterationExternalMetadata_ObjectId",
            schema: "Planning",
            table: "IterationExternalMetadata",
            column: "ObjectId")
            .Annotation("SqlServer:Include", new[] { "Name", "Value" });

        migrationBuilder.CreateIndex(
            name: "IX_Iterations_TeamId",
            schema: "Planning",
            table: "Iterations",
            column: "TeamId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "IterationExternalMetadata",
            schema: "Planning");

        migrationBuilder.DropTable(
            name: "Iterations",
            schema: "Planning");
    }
}
