using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class UpdateWorkflow : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Workflows_IsActive_IsDeleted",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.DropColumn(
            name: "ExternalId",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "Work",
            table: "Workflows",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AddColumn<int>(
            name: "Key",
            schema: "Work",
            table: "Workflows",
            type: "int",
            nullable: false,
            defaultValue: 0)
            .Annotation("SqlServer:Identity", "1, 1");

        migrationBuilder.AddUniqueConstraint(
            name: "AK_Workflows_Key",
            schema: "Work",
            table: "Workflows",
            column: "Key");

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_IsActive_IsDeleted",
            schema: "Work",
            table: "Workflows",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_Key",
            schema: "Work",
            table: "Workflows",
            column: "Key");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropUniqueConstraint(
            name: "AK_Workflows_Key",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.DropIndex(
            name: "IX_Workflows_IsActive_IsDeleted",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.DropIndex(
            name: "IX_Workflows_Key",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.DropColumn(
            name: "Key",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "Work",
            table: "Workflows",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(128)",
            oldMaxLength: 128);

        migrationBuilder.AddColumn<Guid>(
            name: "ExternalId",
            schema: "Work",
            table: "Workflows",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_IsActive_IsDeleted",
            schema: "Work",
            table: "Workflows",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });
    }
}
