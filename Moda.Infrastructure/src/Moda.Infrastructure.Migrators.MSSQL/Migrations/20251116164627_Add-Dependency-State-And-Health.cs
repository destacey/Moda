using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddDependencyStateAndHealth : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Health",
            schema: "Work",
            table: "WorkItemLinks",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "SourcePlannedOn",
            schema: "Work",
            table: "WorkItemLinks",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "SourceStatusCategory",
            schema: "Work",
            table: "WorkItemLinks",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "State",
            schema: "Work",
            table: "WorkItemLinks",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "TargetPlannedOn",
            schema: "Work",
            table: "WorkItemLinks",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "TargetStatusCategory",
            schema: "Work",
            table: "WorkItemLinks",
            type: "int",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Health",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "SourcePlannedOn",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "SourceStatusCategory",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "State",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "TargetPlannedOn",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "TargetStatusCategory",
            schema: "Work",
            table: "WorkItemLinks");
    }
}
