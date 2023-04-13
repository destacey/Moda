using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class RenameTeamMemberhsip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamToTeamMemberships",
                schema: "Organization");

            migrationBuilder.CreateTable(
                name: "TeamMemberships",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Start = table.Column<DateTime>(type: "date", nullable: false),
                    End = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMemberships_Teams_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Organization",
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamMemberships_Teams_TargetId",
                        column: x => x.TargetId,
                        principalSchema: "Organization",
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_Id",
                schema: "Organization",
                table: "TeamMemberships",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "SourceId", "TargetId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_SourceId",
                schema: "Organization",
                table: "TeamMemberships",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_Start_End",
                schema: "Organization",
                table: "TeamMemberships",
                columns: new[] { "Start", "End" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_TargetId",
                schema: "Organization",
                table: "TeamMemberships",
                column: "TargetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamMemberships",
                schema: "Organization");

            migrationBuilder.CreateTable(
                name: "TeamToTeamMemberships",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    End = table.Column<DateTime>(type: "date", nullable: true),
                    Start = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamToTeamMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamToTeamMemberships_Teams_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Organization",
                        principalTable: "Teams",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamToTeamMemberships_Teams_TargetId",
                        column: x => x.TargetId,
                        principalSchema: "Organization",
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamToTeamMemberships_Id",
                schema: "Organization",
                table: "TeamToTeamMemberships",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "SourceId", "TargetId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamToTeamMemberships_SourceId",
                schema: "Organization",
                table: "TeamToTeamMemberships",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamToTeamMemberships_Start_End",
                schema: "Organization",
                table: "TeamToTeamMemberships",
                columns: new[] { "Start", "End" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamToTeamMemberships_TargetId",
                schema: "Organization",
                table: "TeamToTeamMemberships",
                column: "TargetId");
        }
    }
}
