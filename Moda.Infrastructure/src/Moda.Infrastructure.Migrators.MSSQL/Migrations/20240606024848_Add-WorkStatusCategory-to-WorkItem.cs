using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddWorkStatusCategorytoWorkItem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "StatusCategory",
            schema: "Work",
            table: "WorkItems",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true);

        // Update the StatusCategory column with the WorkStatusCategory from the Workflows table
        migrationBuilder.Sql($@"
            UPDATE wi
            SET wi.StatusCategory = wfs.WorkStatusCategory
            FROM [Work].WorkItems wi
                INNER JOIN [Work].[Workspaces] ws ON wi.WorkspaceId = ws.Id
                INNER JOIN [Work].[WorkProcesses] wp ON ws.WorkProcessId = wp.Id
                INNER JOIN [Work].[WorkProcessSchemes] wps ON wp.Id = wps.WorkProcessId AND wi.TypeId = wps.WorkTypeId
                INNER JOIN [Work].[Workflows] wf ON wps.WorkflowId = wf.Id
                INNER JOIN [Work].[WorkflowSchemes] wfs ON wf.Id = wfs.WorkflowId AND wi.StatusId = wfs.WorkStatusId
            WHERE wi.StatusCategory IS NULL");

        migrationBuilder.AlterColumn<string>(
            name: "StatusCategory",
            schema: "Work",
            table: "WorkItems",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32,
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "StatusCategory",
            schema: "Work",
            table: "WorkItems");
    }
}
