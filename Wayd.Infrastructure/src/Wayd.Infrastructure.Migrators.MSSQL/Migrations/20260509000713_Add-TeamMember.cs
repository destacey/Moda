using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddTeamMember : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TeamMemberRoles",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TeamMemberRoles", x => x.Id);
                table.UniqueConstraint("AK_TeamMemberRoles_Key", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "TeamMembers",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TeamMembers", x => x.Id);
                table.ForeignKey(
                    name: "FK_TeamMembers_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_TeamMembers_TeamMemberRoles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "Organization",
                    principalTable: "TeamMemberRoles",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_TeamMembers_Teams_TeamId",
                    column: x => x.TeamId,
                    principalSchema: "Organization",
                    principalTable: "Teams",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberRoles_Name",
            schema: "Organization",
            table: "TeamMemberRoles",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TeamMembers_EmployeeId_IsDeleted",
            schema: "Organization",
            table: "TeamMembers",
            columns: new[] { "EmployeeId", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_TeamMembers_RoleId",
            schema: "Organization",
            table: "TeamMembers",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_TeamMembers_TeamId_EmployeeId_RoleId",
            schema: "Organization",
            table: "TeamMembers",
            columns: new[] { "TeamId", "EmployeeId", "RoleId", "IsDeleted" },
            unique: true,
            filter: "[IsDeleted] = 0");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TeamMembers",
            schema: "Organization");

        migrationBuilder.DropTable(
            name: "TeamMemberRoles",
            schema: "Organization");
    }
}
