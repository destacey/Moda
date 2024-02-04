using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class Updateintegrationrelatedschemas : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Workspaces_Id",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropIndex(
            name: "IX_Workspaces_IsActive_IsDeleted",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropUniqueConstraint(
            name: "AK_WorkProcesses_Key",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcesses_Id",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropColumn(
            name: "Key",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "Work",
            table: "WorkProcesses",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "Planning",
            table: "PlanningIntervals",
            type: "nvarchar(2048)",
            maxLength: 2048,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(1024)",
            oldMaxLength: 1024,
            oldNullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsSyncEnabled",
            schema: "AppIntegrations",
            table: "Connections",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_Id_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Ownership", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_IsActive_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Ownership" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_Key_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Ownership", "IsActive" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Workspaces_Id_IsDeleted",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropIndex(
            name: "IX_Workspaces_IsActive_IsDeleted",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropIndex(
            name: "IX_Workspaces_Key_IsDeleted",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropColumn(
            name: "IsSyncEnabled",
            schema: "AppIntegrations",
            table: "Connections");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            schema: "Work",
            table: "WorkProcesses",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(128)",
            oldMaxLength: 128);

        migrationBuilder.AddColumn<int>(
            name: "Key",
            schema: "Work",
            table: "WorkProcesses",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            schema: "Planning",
            table: "PlanningIntervals",
            type: "nvarchar(1024)",
            maxLength: 1024,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(2048)",
            oldMaxLength: 2048,
            oldNullable: true);

        migrationBuilder.AddUniqueConstraint(
            name: "AK_WorkProcesses_Key",
            schema: "Work",
            table: "WorkProcesses",
            column: "Key");

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_Id",
            schema: "Work",
            table: "Workspaces",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Name", "Ownership", "IsActive", "IsDeleted" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_IsActive_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Ownership" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcesses_Id",
            schema: "Work",
            table: "WorkProcesses",
            column: "Id");
    }
}
