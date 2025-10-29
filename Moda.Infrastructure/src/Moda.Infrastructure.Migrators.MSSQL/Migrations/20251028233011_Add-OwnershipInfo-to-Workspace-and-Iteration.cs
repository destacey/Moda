using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddOwnershipInfotoWorkspaceandIteration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Workspaces_ExternalId",
            schema: "Work",
            table: "Workspaces");

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

        migrationBuilder.AlterColumn<string>(
            name: "ExternalId",
            schema: "Work",
            table: "Workspaces",
            type: "varchar(64)",
            maxLength: 64,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Connector",
            schema: "Work",
            table: "Workspaces",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SystemId",
            schema: "Work",
            table: "Workspaces",
            type: "varchar(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemId",
            schema: "Planning",
            table: "Iterations",
            type: "varchar(64)",
            maxLength: 64,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(128)",
            oldMaxLength: 128,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ExternalId",
            schema: "Planning",
            table: "Iterations",
            type: "varchar(64)",
            maxLength: 64,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(128)",
            oldMaxLength: 128,
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Connector",
            schema: "Planning",
            table: "Iterations",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemId",
            schema: "AppIntegrations",
            table: "Connections",
            type: "varchar(64)",
            maxLength: 64,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_Id_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_IsActive_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_Key_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "IsActive" });


        migrationBuilder.Sql("UPDATE [Planning].[Iterations] SET Connector = 'AzureDevOps' WHERE Connector IS NULL AND Ownership = 'Managed';");

        migrationBuilder.Sql("UPDATE [Work].[Workspaces] SET Connector = 'AzureDevOps' WHERE Connector IS NULL AND Ownership = 'Managed';");

        migrationBuilder.Sql("UPDATE [AppIntegrations].[Connections] SET Connector = 'AzureDevOps' WHERE Connector = 'AzureDevOpsBoards';");
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
            name: "Connector",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropColumn(
            name: "SystemId",
            schema: "Work",
            table: "Workspaces");

        migrationBuilder.DropColumn(
            name: "Connector",
            schema: "Planning",
            table: "Iterations");

        migrationBuilder.AlterColumn<Guid>(
            name: "ExternalId",
            schema: "Work",
            table: "Workspaces",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(64)",
            oldMaxLength: 64,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemId",
            schema: "Planning",
            table: "Iterations",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(64)",
            oldMaxLength: 64,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ExternalId",
            schema: "Planning",
            table: "Iterations",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(64)",
            oldMaxLength: 64,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemId",
            schema: "AppIntegrations",
            table: "Connections",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(64)",
            oldMaxLength: 64,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_ExternalId",
            schema: "Work",
            table: "Workspaces",
            column: "ExternalId",
            filter: "[ExternalId] IS NOT NULL")
            .Annotation("SqlServer:Include", new[] { "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_Id_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Ownership", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_IsActive_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Ownership" });

        migrationBuilder.CreateIndex(
            name: "IX_Workspaces_Key_IsDeleted",
            schema: "Work",
            table: "Workspaces",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Ownership", "IsActive" });
    }
}
