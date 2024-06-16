using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddIsDeletedfiltertoindexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkTypeLevels_WorkTypeHierarchies_WorkTypeHierarchyId",
            schema: "Work",
            table: "WorkTypeLevels");

        migrationBuilder.DropIndex(
            name: "IX_WorkTypes_Id",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.DropIndex(
            name: "IX_WorkTypes_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.DropIndex(
            name: "IX_WorkStatuses_Id",
            schema: "Work",
            table: "WorkStatuses");

        migrationBuilder.DropIndex(
            name: "IX_WorkStatuses_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkStatuses");

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

        migrationBuilder.DropIndex(
            name: "IX_WorkProcessSchemes_Id",
            schema: "Work",
            table: "WorkProcessSchemes");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcessSchemes_WorkProcessId",
            schema: "Work",
            table: "WorkProcessSchemes");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcesses_Id_IsDeleted",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcesses_Key_IsDeleted",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_WorkspaceId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkflowSchemes_Id",
            schema: "Work",
            table: "WorkflowSchemes");

        migrationBuilder.DropIndex(
            name: "IX_WorkflowSchemes_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkflowSchemes");

        migrationBuilder.DropIndex(
            name: "IX_WorkflowSchemes_WorkflowId",
            schema: "Work",
            table: "WorkflowSchemes");

        migrationBuilder.DropIndex(
            name: "IX_Workflows_Id",
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

        migrationBuilder.DropIndex(
            name: "IX_Teams_Id",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.DropIndex(
            name: "IX_Teams_IsActive",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.DropIndex(
            name: "IX_Teams_IsDeleted",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.DropIndex(
            name: "IX_Teams_Key",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.DropIndex(
            name: "IX_TeamMemberships_Id",
            schema: "Organization",
            table: "TeamMemberships");

        migrationBuilder.DropIndex(
            name: "IX_Risks_AssigneeId",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_Risks_Id",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_Risks_IsDeleted",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_Risks_TeamId",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_IsActive_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervals_Id",
            schema: "Planning",
            table: "PlanningIntervals");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervals_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervals");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_ObjectiveId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_Id_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_Key_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_OwnerId_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_PlanId_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Employees_Id",
            schema: "Organization",
            table: "Employees");

        migrationBuilder.DropIndex(
            name: "IX_Employees_IsActive",
            schema: "Organization",
            table: "Employees");

        migrationBuilder.DropIndex(
            name: "IX_Employees_IsDeleted",
            schema: "Organization",
            table: "Employees");

        migrationBuilder.DropIndex(
            name: "IX_Connections_Connector_IsActive",
            schema: "AppIntegrations",
            table: "Connections");

        migrationBuilder.DropIndex(
            name: "IX_Connections_Id",
            schema: "AppIntegrations",
            table: "Connections");

        migrationBuilder.DropIndex(
            name: "IX_Connections_IsActive",
            schema: "AppIntegrations",
            table: "Connections");

        migrationBuilder.DropIndex(
            name: "IX_Connections_IsDeleted",
            schema: "AppIntegrations",
            table: "Connections");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypes_Id_IsDeleted",
            schema: "Work",
            table: "WorkTypes",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Name", "LevelId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypes_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkTypes",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "LevelId", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkStatuses_Id_IsDeleted",
            schema: "Work",
            table: "WorkStatuses",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_WorkStatuses_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkStatuses",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

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

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcessSchemes_Id_IsDeleted",
            schema: "Work",
            table: "WorkProcessSchemes",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcessSchemes_WorkProcessId_IsDeleted",
            schema: "Work",
            table: "WorkProcessSchemes",
            columns: new[] { "WorkProcessId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "WorkTypeId", "WorkflowId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcesses_Id_IsDeleted",
            schema: "Work",
            table: "WorkProcesses",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "ExternalId", "Ownership", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcesses_Key_IsDeleted",
            schema: "Work",
            table: "WorkProcesses",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "ExternalId", "Ownership", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_WorkspaceId",
            schema: "Work",
            table: "WorkItems",
            column: "WorkspaceId")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Title", "ExternalId", "AssignedToId", "TypeId", "StatusId", "StatusCategory", "ActivatedTimestamp", "DoneTimestamp" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_Id_IsDeleted",
            schema: "Work",
            table: "WorkflowSchemes",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_WorkflowId_IsDeleted",
            schema: "Work",
            table: "WorkflowSchemes",
            columns: new[] { "WorkflowId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "WorkStatusId", "WorkStatusCategory" });

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_Id_IsDeleted",
            schema: "Work",
            table: "Workflows",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_IsActive_IsDeleted",
            schema: "Work",
            table: "Workflows",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_Key_IsDeleted",
            schema: "Work",
            table: "Workflows",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_Id_IsDeleted",
            schema: "Organization",
            table: "Teams",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Teams_IsActive_IsDeleted",
            schema: "Organization",
            table: "Teams",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_Key_IsDeleted",
            schema: "Organization",
            table: "Teams",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberships_Id_IsDeleted",
            schema: "Organization",
            table: "TeamMemberships",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "SourceId", "TargetId" });

        migrationBuilder.CreateIndex(
            name: "IX_Risks_AssigneeId_IsDeleted",
            schema: "Planning",
            table: "Risks",
            columns: new[] { "AssigneeId", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_Id_IsDeleted",
            schema: "Planning",
            table: "Risks",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_Key_IsDeleted",
            schema: "Planning",
            table: "Risks",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_TeamId_IsDeleted",
            schema: "Planning",
            table: "Risks",
            columns: new[] { "TeamId", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_IsActive_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervals_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervals",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Name", "Description" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "PlanningIntervalId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "PlanningIntervalId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_ObjectiveId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            columns: new[] { "ObjectiveId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "PlanningIntervalId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            columns: new[] { "PlanningIntervalId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "PlanningIntervalId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_Id_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Type", "Status", "OwnerId", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_Key_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "Key", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Type", "Status", "OwnerId", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_OwnerId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "OwnerId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_PlanId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "PlanId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Employees_Id_IsDeleted",
            schema: "Organization",
            table: "Employees",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_IsActive_IsDeleted",
            schema: "Organization",
            table: "Employees",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Connections_Connector_IsActive_IsDeleted",
            schema: "AppIntegrations",
            table: "Connections",
            columns: new[] { "Connector", "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_Connections_Id_IsDeleted",
            schema: "AppIntegrations",
            table: "Connections",
            columns: new[] { "Id", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_Connections_IsActive_IsDeleted",
            schema: "AppIntegrations",
            table: "Connections",
            columns: new[] { "IsActive", "IsDeleted" },
            filter: "[IsDeleted] = 0");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkTypeLevels_WorkTypeHierarchies_WorkTypeHierarchyId",
            schema: "Work",
            table: "WorkTypeLevels",
            column: "WorkTypeHierarchyId",
            principalSchema: "Work",
            principalTable: "WorkTypeHierarchies",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkTypeLevels_WorkTypeHierarchies_WorkTypeHierarchyId",
            schema: "Work",
            table: "WorkTypeLevels");

        migrationBuilder.DropIndex(
            name: "IX_WorkTypes_Id_IsDeleted",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.DropIndex(
            name: "IX_WorkTypes_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.DropIndex(
            name: "IX_WorkStatuses_Id_IsDeleted",
            schema: "Work",
            table: "WorkStatuses");

        migrationBuilder.DropIndex(
            name: "IX_WorkStatuses_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkStatuses");

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

        migrationBuilder.DropIndex(
            name: "IX_WorkProcessSchemes_Id_IsDeleted",
            schema: "Work",
            table: "WorkProcessSchemes");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcessSchemes_WorkProcessId_IsDeleted",
            schema: "Work",
            table: "WorkProcessSchemes");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcesses_Id_IsDeleted",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropIndex(
            name: "IX_WorkProcesses_Key_IsDeleted",
            schema: "Work",
            table: "WorkProcesses");

        migrationBuilder.DropIndex(
            name: "IX_WorkItems_WorkspaceId",
            schema: "Work",
            table: "WorkItems");

        migrationBuilder.DropIndex(
            name: "IX_WorkflowSchemes_Id_IsDeleted",
            schema: "Work",
            table: "WorkflowSchemes");

        migrationBuilder.DropIndex(
            name: "IX_WorkflowSchemes_WorkflowId_IsDeleted",
            schema: "Work",
            table: "WorkflowSchemes");

        migrationBuilder.DropIndex(
            name: "IX_Workflows_Id_IsDeleted",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.DropIndex(
            name: "IX_Workflows_IsActive_IsDeleted",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.DropIndex(
            name: "IX_Workflows_Key_IsDeleted",
            schema: "Work",
            table: "Workflows");

        migrationBuilder.DropIndex(
            name: "IX_Teams_Id_IsDeleted",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.DropIndex(
            name: "IX_Teams_IsActive_IsDeleted",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.DropIndex(
            name: "IX_Teams_Key_IsDeleted",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.DropIndex(
            name: "IX_TeamMemberships_Id_IsDeleted",
            schema: "Organization",
            table: "TeamMemberships");

        migrationBuilder.DropIndex(
            name: "IX_Risks_AssigneeId_IsDeleted",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_Risks_Id_IsDeleted",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_Risks_Key_IsDeleted",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_Risks_TeamId_IsDeleted",
            schema: "Planning",
            table: "Risks");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_IsActive_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningTeams_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervals_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervals");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_ObjectiveId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalObjectives_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_Id_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_Key_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_OwnerId_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_PlanId_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Employees_Id_IsDeleted",
            schema: "Organization",
            table: "Employees");

        migrationBuilder.DropIndex(
            name: "IX_Employees_IsActive_IsDeleted",
            schema: "Organization",
            table: "Employees");

        migrationBuilder.DropIndex(
            name: "IX_Connections_Connector_IsActive_IsDeleted",
            schema: "AppIntegrations",
            table: "Connections");

        migrationBuilder.DropIndex(
            name: "IX_Connections_Id_IsDeleted",
            schema: "AppIntegrations",
            table: "Connections");

        migrationBuilder.DropIndex(
            name: "IX_Connections_IsActive_IsDeleted",
            schema: "AppIntegrations",
            table: "Connections");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypes_Id",
            schema: "Work",
            table: "WorkTypes",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypes_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkTypes",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkStatuses_Id",
            schema: "Work",
            table: "WorkStatuses",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkStatuses_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkStatuses",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

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

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcessSchemes_Id",
            schema: "Work",
            table: "WorkProcessSchemes",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkProcessSchemes_WorkProcessId",
            schema: "Work",
            table: "WorkProcessSchemes",
            column: "WorkProcessId");

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

        migrationBuilder.CreateIndex(
            name: "IX_WorkItems_WorkspaceId",
            schema: "Work",
            table: "WorkItems",
            column: "WorkspaceId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_Id",
            schema: "Work",
            table: "WorkflowSchemes",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkflowSchemes",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowSchemes_WorkflowId",
            schema: "Work",
            table: "WorkflowSchemes",
            column: "WorkflowId");

        migrationBuilder.CreateIndex(
            name: "IX_Workflows_Id",
            schema: "Work",
            table: "Workflows",
            column: "Id");

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

        migrationBuilder.CreateIndex(
            name: "IX_Teams_Id",
            schema: "Organization",
            table: "Teams",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "IsActive", "IsDeleted" });

        migrationBuilder.CreateIndex(
            name: "IX_Teams_IsActive",
            schema: "Organization",
            table: "Teams",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_Teams_IsDeleted",
            schema: "Organization",
            table: "Teams",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Teams_Key",
            schema: "Organization",
            table: "Teams",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "IsActive", "IsDeleted" });

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberships_Id",
            schema: "Organization",
            table: "TeamMemberships",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "SourceId", "TargetId", "IsDeleted" });

        migrationBuilder.CreateIndex(
            name: "IX_Risks_AssigneeId",
            schema: "Planning",
            table: "Risks",
            column: "AssigneeId");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_Id",
            schema: "Planning",
            table: "Risks",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_IsDeleted",
            schema: "Planning",
            table: "Risks",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_TeamId",
            schema: "Planning",
            table: "Risks",
            column: "TeamId");

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_IsActive_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningTeams_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningTeams",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervals_Id",
            schema: "Planning",
            table: "PlanningIntervals",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Name", "Description" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervals_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervals",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "PlanningIntervalId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "PlanningIntervalId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "PlanningIntervalId", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_ObjectiveId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            columns: new[] { "ObjectiveId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "PlanningIntervalId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalObjectives_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalObjectives",
            columns: new[] { "PlanningIntervalId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "ObjectiveId", "Type", "IsStretch" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Id_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_Key_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "PlanningIntervalId", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PlanningIntervalIterations_PlanningIntervalId_IsDeleted",
            schema: "Planning",
            table: "PlanningIntervalIterations",
            columns: new[] { "PlanningIntervalId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_Id_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Type", "Status", "OwnerId", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_Key_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Type", "Status", "OwnerId", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_OwnerId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "OwnerId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_PlanId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "PlanId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Employees_Id",
            schema: "Organization",
            table: "Employees",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_IsActive",
            schema: "Organization",
            table: "Employees",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_IsDeleted",
            schema: "Organization",
            table: "Employees",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Connections_Connector_IsActive",
            schema: "AppIntegrations",
            table: "Connections",
            columns: new[] { "Connector", "IsActive" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_Connections_Id",
            schema: "AppIntegrations",
            table: "Connections",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Connections_IsActive",
            schema: "AppIntegrations",
            table: "Connections",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_Connections_IsDeleted",
            schema: "AppIntegrations",
            table: "Connections",
            column: "IsDeleted");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkTypeLevels_WorkTypeHierarchies_WorkTypeHierarchyId",
            schema: "Work",
            table: "WorkTypeLevels",
            column: "WorkTypeHierarchyId",
            principalSchema: "Work",
            principalTable: "WorkTypeHierarchies",
            principalColumn: "Id");
    }
}
