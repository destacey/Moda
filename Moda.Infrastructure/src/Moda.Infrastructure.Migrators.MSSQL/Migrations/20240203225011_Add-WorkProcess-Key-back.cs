using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddWorkProcessKeyback : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Key",
            schema: "Work",
            table: "WorkProcesses",
            type: "int",
            nullable: false,
            defaultValue: 0)
            .Annotation("SqlServer:Identity", "1, 1");

        migrationBuilder.AddUniqueConstraint(
            name: "AK_WorkProcesses_Key",
            schema: "Work",
            table: "WorkProcesses",
            column: "Key");

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcesses_Id_IsDeleted",
            schema: "Work",
            table: "WorkProcesses",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "ExternalId", "Ownership", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcesses_Key_IsDeleted",
            schema: "Work",
            table: "WorkProcesses",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "ExternalId", "Ownership", "IsActive" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropUniqueConstraint(
            name: "AK_WorkProcesses_Key",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcesses_Id_IsDeleted",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcesses_Key_IsDeleted",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropColumn(
            name: "Key",
            schema: "Work",
            table: "WorkProcesses");
    }
}
