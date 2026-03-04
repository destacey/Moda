using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class ChangeUserIdFromGuidToString : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkTypes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkTypes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkTypes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkTypeLevels",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkTypeLevels",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkTypeHierarchies",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkTypeHierarchies",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkStatuses",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkStatuses",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkStatuses",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "Workspaces",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Work",
            table: "Workspaces",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Work",
            table: "Workspaces",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkProcesses",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkProcesses",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkProcesses",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkItems",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkItems",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkItemReferences",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkItemReferences",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkItemLinks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkItemLinks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "Workflows",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Work",
            table: "Workflows",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Work",
            table: "Workflows",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "StrategicManagement",
            table: "Visions",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "StrategicManagement",
            table: "Visions",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "Teams",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Organization",
            table: "Teams",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Organization",
            table: "Teams",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Organization",
            table: "TeamOperatingModels",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Organization",
            table: "TeamOperatingModels",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "TeamMemberships",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Organization",
            table: "TeamMemberships",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Organization",
            table: "TeamMemberships",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "StrategicManagement",
            table: "Strategies",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "StrategicManagement",
            table: "Strategies",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "StrategicManagement",
            table: "StrategicThemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "StrategicManagement",
            table: "StrategicThemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiatives",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiatives",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiativeRoleAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiativeRoleAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpiMeasurements",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpiMeasurements",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpiCheckpoints",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpiCheckpoints",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "Roadmaps",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Planning",
            table: "Roadmaps",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "RoadmapItems",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "RoadmapItems",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "Risks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Planning",
            table: "Risks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Planning",
            table: "Risks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectTasks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectTasks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectTaskDependencies",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectTaskDependencies",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectTaskAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectTaskAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectStrategicThemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectStrategicThemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "Projects",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "Projects",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectRoleAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectRoleAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProgramStrategicThemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProgramStrategicThemes",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "Programs",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "Programs",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProgramRoleAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProgramRoleAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "Portfolios",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "Portfolios",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "PortfolioRoleAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "PortfolioRoleAssignments",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "PokerSessions",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "PokerSessions",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalIterationSprints",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "PlanningIntervalIterationSprints",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Identity",
            table: "PersonalAccessTokens",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Identity",
            table: "PersonalAccessTokens",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "RevokedBy",
            schema: "Identity",
            table: "PersonalAccessTokens",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Goals",
            table: "Objectives",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Goals",
            table: "Objectives",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Goals",
            table: "Objectives",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "Iterations",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "Iterations",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "IterationExternalMetadata",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "IterationExternalMetadata",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Health",
            table: "HealthChecks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Health",
            table: "HealthChecks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Health",
            table: "HealthChecks",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ExpenditureCategories",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ExpenditureCategories",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "EstimationScales",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "EstimationScales",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "Employees",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "Organization",
            table: "Employees",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "Organization",
            table: "Employees",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastModifiedBy",
            schema: "AppIntegrations",
            table: "Connections",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DeletedBy",
            schema: "AppIntegrations",
            table: "Connections",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "CreatedBy",
            schema: "AppIntegrations",
            table: "Connections",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            schema: "Auditing",
            table: "AuditTrails",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkTypes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkTypes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkTypes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkTypeLevels",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkTypeLevels",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkTypeHierarchies",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkTypeHierarchies",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkStatuses",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkStatuses",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkStatuses",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "Workspaces",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Work",
            table: "Workspaces",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Work",
            table: "Workspaces",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkProcesses",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkProcesses",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkProcesses",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkItems",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkItemReferences",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkItemReferences",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkItemLinks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkItemLinks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Work",
            table: "Workflows",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Work",
            table: "Workflows",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Work",
            table: "Workflows",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "StrategicManagement",
            table: "Visions",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "StrategicManagement",
            table: "Visions",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "Teams",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Organization",
            table: "Teams",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Organization",
            table: "Teams",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Organization",
            table: "TeamOperatingModels",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Organization",
            table: "TeamOperatingModels",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "TeamMemberships",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Organization",
            table: "TeamMemberships",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Organization",
            table: "TeamMemberships",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "StrategicManagement",
            table: "Strategies",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "StrategicManagement",
            table: "Strategies",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "StrategicManagement",
            table: "StrategicThemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "StrategicManagement",
            table: "StrategicThemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiatives",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiatives",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiativeRoleAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiativeRoleAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpis",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpiMeasurements",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpiMeasurements",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpiCheckpoints",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "StrategicInitiativeKpiCheckpoints",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "Roadmaps",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Planning",
            table: "Roadmaps",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "RoadmapItems",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "RoadmapItems",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "Risks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Planning",
            table: "Risks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Planning",
            table: "Risks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectTasks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectTasks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectTaskDependencies",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectTaskDependencies",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectTaskAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectTaskAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectStrategicThemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectStrategicThemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "Projects",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "Projects",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProjectRoleAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProjectRoleAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProgramStrategicThemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProgramStrategicThemes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "Programs",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "Programs",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ProgramRoleAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ProgramRoleAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "Portfolios",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "Portfolios",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "PortfolioRoleAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "PortfolioRoleAssignments",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "PokerSessions",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "PokerSessions",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalIterationSprints",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "PlanningIntervalIterationSprints",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Identity",
            table: "PersonalAccessTokens",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Identity",
            table: "PersonalAccessTokens",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "RevokedBy",
            schema: "Identity",
            table: "PersonalAccessTokens",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Goals",
            table: "Objectives",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Goals",
            table: "Objectives",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Goals",
            table: "Objectives",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "Iterations",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "Iterations",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "IterationExternalMetadata",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "IterationExternalMetadata",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Health",
            table: "HealthChecks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Health",
            table: "HealthChecks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Health",
            table: "HealthChecks",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Ppm",
            table: "ExpenditureCategories",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Ppm",
            table: "ExpenditureCategories",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "EstimationScales",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "EstimationScales",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "Employees",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "Organization",
            table: "Employees",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "Organization",
            table: "Employees",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "LastModifiedBy",
            schema: "AppIntegrations",
            table: "Connections",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "DeletedBy",
            schema: "AppIntegrations",
            table: "Connections",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            schema: "AppIntegrations",
            table: "Connections",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "UserId",
            schema: "Auditing",
            table: "AuditTrails",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450);
    }
}
