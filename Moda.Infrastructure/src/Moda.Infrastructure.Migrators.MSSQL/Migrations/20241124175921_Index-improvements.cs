using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class Indeximprovements : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Tier",
            schema: "Work",
            table: "WorkTypeLevels",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(32)",
            oldMaxLength: 32);

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypes_Id",
            schema: "Work",
            table: "WorkTypes",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "LevelId", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypes_Name_IsDeleted",
            schema: "Work",
            table: "WorkTypes",
            columns: new[] { "Name", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "LevelId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypeLevels_Id_Tier",
            schema: "Work",
            table: "WorkTypeLevels",
            columns: new[] { "Id", "Tier" })
            .Annotation("SqlServer:Include", new[] { "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_ExternalId",
            schema: "Work",
            table: "Workspaces",
            column: "ExternalId",
            filter: "[ExternalId] IS NOT NULL")
            .Annotation("SqlServer:Include", new[] { "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_WorkspaceId_ExternalId",
            schema: "Work",
            table: "WorkItems",
            columns: new[] { "WorkspaceId", "ExternalId" },
            filter: "[ExternalId] IS NOT NULL")
            .Annotation("SqlServer:Include", new[] { "Id", "ParentId", "TypeId" });

        migrationBuilder.CreateIndex(
            name: "IX_Employees_Email",
            schema: "Organization",
            table: "Employees",
            column: "Email")
            .Annotation("SqlServer:Include", new[] { "Id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_WorkTypes_Id",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.DropIndex(
            name: "IX_WorkTypes_Name_IsDeleted",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.DropIndex(
            name: "IX_WorkTypeLevels_Id_Tier",
            schema: "Work",
            table: "WorkTypeLevels");

        migrationBuilder.DropIndex(
            name: "IX_Workspaces_ExternalId",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_WorkspaceId_ExternalId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_Employees_Email",
            schema: "Organization",
            table: "Employees");

        migrationBuilder.AlterColumn<string>(
            name: "Tier",
            schema: "Work",
            table: "WorkTypeLevels",
            type: "nvarchar(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);
    }
}
