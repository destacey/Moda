using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RefactorAuditableToSystemAuditable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkTypes",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Work",
            table: "WorkTypes",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkTypes",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Work",
            table: "WorkTypes",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkStatuses",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Work",
            table: "WorkStatuses",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkStatuses",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Work",
            table: "WorkStatuses",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Work",
            table: "Workspaces",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Work",
            table: "Workspaces",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Work",
            table: "Workspaces",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Work",
            table: "Workspaces",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Work",
            table: "WorkProcessSchemes",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Work",
            table: "WorkProcessSchemes",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkProcesses",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Work",
            table: "WorkProcesses",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkProcesses",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Work",
            table: "WorkProcesses",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Work",
            table: "WorkflowSchemes",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Work",
            table: "WorkflowSchemes",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Work",
            table: "Workflows",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Work",
            table: "Workflows",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Work",
            table: "Workflows",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Work",
            table: "Workflows",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "Teams",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Organization",
            table: "Teams",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Organization",
            table: "Teams",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Organization",
            table: "Teams",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "TeamMemberships",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Organization",
            table: "TeamMemberships",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Organization",
            table: "TeamMemberships",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Organization",
            table: "TeamMemberships",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "Roadmaps",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Planning",
            table: "Roadmaps",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Planning",
            table: "Roadmaps",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Planning",
            table: "Roadmaps",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "Risks",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Planning",
            table: "Risks",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Planning",
            table: "Risks",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Planning",
            table: "Risks",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Planning",
            table: "PlanningIntervals",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Planning",
            table: "PlanningIntervals",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Goals",
            table: "Objectives",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Goals",
            table: "Objectives",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Goals",
            table: "Objectives",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Goals",
            table: "Objectives",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Health",
            table: "HealthChecks",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Health",
            table: "HealthChecks",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Health",
            table: "HealthChecks",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Health",
            table: "HealthChecks",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "Organization",
            table: "Employees",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "Organization",
            table: "Employees",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "Organization",
            table: "Employees",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "Organization",
            table: "Employees",
            newName: "SystemCreated");

        migrationBuilder.RenameColumn(
            name: "LastModifiedBy",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "SystemLastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "LastModified",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "SystemLastModified");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "SystemCreatedBy");

        migrationBuilder.RenameColumn(
            name: "Created",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "SystemCreated");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkTypes",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Work",
            table: "WorkTypes",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkTypes",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Work",
            table: "WorkTypes",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkStatuses",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Work",
            table: "WorkStatuses",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkStatuses",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Work",
            table: "WorkStatuses",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "Workspaces",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Work",
            table: "Workspaces",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "Workspaces",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Work",
            table: "Workspaces",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Work",
            table: "WorkProcessSchemes",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkProcessSchemes",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Work",
            table: "WorkProcessSchemes",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkProcesses",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Work",
            table: "WorkProcesses",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkProcesses",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Work",
            table: "WorkProcesses",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Work",
            table: "WorkflowSchemes",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "WorkflowSchemes",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Work",
            table: "WorkflowSchemes",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Work",
            table: "Workflows",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Work",
            table: "Workflows",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Work",
            table: "Workflows",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Work",
            table: "Workflows",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Organization",
            table: "Teams",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Organization",
            table: "Teams",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Organization",
            table: "Teams",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Organization",
            table: "Teams",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Organization",
            table: "TeamMemberships",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Organization",
            table: "TeamMemberships",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Organization",
            table: "TeamMemberships",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Organization",
            table: "TeamMemberships",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "Roadmaps",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Planning",
            table: "Roadmaps",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "Roadmaps",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Planning",
            table: "Roadmaps",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "Risks",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Planning",
            table: "Risks",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "Risks",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Planning",
            table: "Risks",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Planning",
            table: "PlanningIntervals",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "PlanningIntervals",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Planning",
            table: "PlanningIntervals",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Goals",
            table: "Objectives",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Goals",
            table: "Objectives",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Goals",
            table: "Objectives",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Goals",
            table: "Objectives",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Health",
            table: "HealthChecks",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Health",
            table: "HealthChecks",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Health",
            table: "HealthChecks",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Health",
            table: "HealthChecks",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "Organization",
            table: "Employees",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "Organization",
            table: "Employees",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "Organization",
            table: "Employees",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "Organization",
            table: "Employees",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "SystemLastModifiedBy",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "LastModifiedBy");

        migrationBuilder.RenameColumn(
            name: "SystemLastModified",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "LastModified");

        migrationBuilder.RenameColumn(
            name: "SystemCreatedBy",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "CreatedBy");

        migrationBuilder.RenameColumn(
            name: "SystemCreated",
            schema: "AppIntegrations",
            table: "Connections",
            newName: "Created");
    }
}
