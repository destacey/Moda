using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddTeamMemberRoleDescription : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamMemberRoles_Name",
            schema: "Organization",
            table: "TeamMemberRoles");

        migrationBuilder.AddColumn<string>(
            name: "Description",
            schema: "Organization",
            table: "TeamMemberRoles",
            type: "nvarchar(1024)",
            maxLength: 1024,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberRoles_Name",
            schema: "Organization",
            table: "TeamMemberRoles",
            column: "Name",
            unique: true,
            filter: "[IsDeleted] = 0");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamMemberRoles_Name",
            schema: "Organization",
            table: "TeamMemberRoles");

        migrationBuilder.DropColumn(
            name: "Description",
            schema: "Organization",
            table: "TeamMemberRoles");

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberRoles_Name",
            schema: "Organization",
            table: "TeamMemberRoles",
            column: "Name",
            unique: true);
    }
}
