using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddPlanningPokerAndUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {         
        migrationBuilder.Sql("""
                CREATE VIEW [Identity].[vw_ModaUsers] AS
                SELECT
                    u.Id,
                    u.UserName,
                    u.FirstName,
                    u.LastName,
                    CONCAT_WS(' ', u.FirstName, u.LastName) AS DisplayName,
                    u.Email,
                    u.IsActive
                FROM [Identity].[Users] u
                """);

        migrationBuilder.CreateTable(
            name: "EstimationScales",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                Values = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EstimationScales", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PokerSessions",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                EstimationScaleId = table.Column<int>(type: "int", nullable: false),
                FacilitatorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                ActivatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PokerSessions", x => x.Id);
                table.UniqueConstraint("AK_PokerSessions_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_PokerSessions_EstimationScales_EstimationScaleId",
                    column: x => x.EstimationScaleId,
                    principalSchema: "Planning",
                    principalTable: "EstimationScales",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PokerRounds",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PokerSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Label = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                ConsensusEstimate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                Order = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PokerRounds", x => x.Id);
                table.ForeignKey(
                    name: "FK_PokerRounds_PokerSessions_PokerSessionId",
                    column: x => x.PokerSessionId,
                    principalSchema: "Planning",
                    principalTable: "PokerSessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PokerVotes",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PokerRoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParticipantId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Value = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                SubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PokerVotes", x => x.Id);
                table.ForeignKey(
                    name: "FK_PokerVotes_PokerRounds_PokerRoundId",
                    column: x => x.PokerRoundId,
                    principalSchema: "Planning",
                    principalTable: "PokerRounds",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PokerRounds_PokerSessionId",
            schema: "Planning",
            table: "PokerRounds",
            column: "PokerSessionId");

        migrationBuilder.CreateIndex(
            name: "IX_PokerSessions_EstimationScaleId",
            schema: "Planning",
            table: "PokerSessions",
            column: "EstimationScaleId");

        migrationBuilder.CreateIndex(
            name: "IX_PokerSessions_FacilitatorId",
            schema: "Planning",
            table: "PokerSessions",
            column: "FacilitatorId");

        migrationBuilder.CreateIndex(
            name: "IX_PokerSessions_Key",
            schema: "Planning",
            table: "PokerSessions",
            column: "Key");

        migrationBuilder.CreateIndex(
            name: "IX_PokerVotes_ParticipantId",
            schema: "Planning",
            table: "PokerVotes",
            column: "ParticipantId");

        migrationBuilder.CreateIndex(
            name: "IX_PokerVotes_PokerRoundId_ParticipantId",
            schema: "Planning",
            table: "PokerVotes",
            columns: new[] { "PokerRoundId", "ParticipantId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PokerVotes",
            schema: "Planning");

        migrationBuilder.DropTable(
            name: "PokerRounds",
            schema: "Planning");

        migrationBuilder.DropTable(
            name: "PokerSessions",
            schema: "Planning");

        migrationBuilder.DropTable(
            name: "EstimationScales",
            schema: "Planning");

        migrationBuilder.Sql("""
            DROP VIEW [Identity].[vw_ModaUsers]
            """);
    }
}
