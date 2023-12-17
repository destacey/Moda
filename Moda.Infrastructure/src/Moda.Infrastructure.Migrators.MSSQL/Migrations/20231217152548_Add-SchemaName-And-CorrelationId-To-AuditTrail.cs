using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddSchemaNameAndCorrelationIdToAuditTrail : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Auditing",
            table: "AuditTrails",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TableName",
            schema: "Auditing",
            table: "AuditTrails",
            type: "varchar(128)",
            maxLength: 128,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "PrimaryKey",
            schema: "Auditing",
            table: "AuditTrails",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "CorrelationId",
            schema: "Auditing",
            table: "AuditTrails",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SchemaName",
            schema: "Auditing",
            table: "AuditTrails",
            type: "varchar(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AuditTrails_PrimaryKey",
            schema: "Auditing",
            table: "AuditTrails",
            column: "PrimaryKey");

        migrationBuilder.CreateIndex(
            name: "IX_AuditTrails_UserId",
            schema: "Auditing",
            table: "AuditTrails",
            column: "UserId");

        // Add SchemaName to existing AuditTrails

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [SchemaName] = 'AppIntegrations'
            WHERE [TableName] = 'AzureDevOpsBoardsConnection'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [SchemaName] = 'Goals'
            WHERE [TableName] = 'Objective'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [SchemaName] = 'Health'
            WHERE [TableName] = 'HealthCheck'");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [SchemaName] = 'Work'
            WHERE [TableName] IN ('BacklogLevel','BacklogLevelScheme')");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [SchemaName] = 'Organization'
            WHERE [TableName] IN ('Employee','Team','TeamOfTeams','TeamMembership')");

        migrationBuilder.Sql(@"
            UPDATE [Auditing].[AuditTrails]
            SET [SchemaName] = 'Organization'
            WHERE [TableName] IN ('PlanningInterval','PlanningIntervalObjective','Risk')");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AuditTrails_PrimaryKey",
            schema: "Auditing",
            table: "AuditTrails");

        migrationBuilder.DropIndex(
            name: "IX_AuditTrails_UserId",
            schema: "Auditing",
            table: "AuditTrails");

        migrationBuilder.DropColumn(
            name: "CorrelationId",
            schema: "Auditing",
            table: "AuditTrails");

        migrationBuilder.DropColumn(
            name: "SchemaName",
            schema: "Auditing",
            table: "AuditTrails");

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            schema: "Auditing",
            table: "AuditTrails",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TableName",
            schema: "Auditing",
            table: "AuditTrails",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(128)",
            oldMaxLength: 128,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "PrimaryKey",
            schema: "Auditing",
            table: "AuditTrails",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);
    }
}
