using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddProjectPhases : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add ProjectPhaseId as nullable first — will be made non-nullable after data migration
        migrationBuilder.AddColumn<Guid>(
            name: "ProjectPhaseId",
            schema: "Ppm",
            table: "ProjectTasks",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ProjectLifecycleId",
            schema: "Ppm",
            table: "Projects",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ProjectLifecycles",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectLifecycles", x => x.Id);
                table.UniqueConstraint("AK_ProjectLifecycles_Key", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "ProjectLifecyclePhases",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProjectLifecycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectLifecyclePhases", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProjectLifecyclePhases_ProjectLifecycles_ProjectLifecycleId",
                    column: x => x.ProjectLifecycleId,
                    principalSchema: "Ppm",
                    principalTable: "ProjectLifecycles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProjectPhases",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProjectLifecyclePhaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                Start = table.Column<DateTime>(type: "date", nullable: true),
                End = table.Column<DateTime>(type: "date", nullable: true),
                Progress = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectPhases", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProjectPhases_ProjectLifecyclePhases_ProjectLifecyclePhaseId",
                    column: x => x.ProjectLifecyclePhaseId,
                    principalSchema: "Ppm",
                    principalTable: "ProjectLifecyclePhases",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProjectPhases_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalSchema: "Ppm",
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProjectPhaseAssignments",
            schema: "Ppm",
            columns: table => new
            {
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Role = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectPhaseAssignments", x => new { x.ObjectId, x.EmployeeId, x.Role });
                table.ForeignKey(
                    name: "FK_ProjectPhaseAssignments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProjectPhaseAssignments_ProjectPhases_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Ppm",
                    principalTable: "ProjectPhases",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTasks_ProjectPhaseId",
            schema: "Ppm",
            table: "ProjectTasks",
            column: "ProjectPhaseId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_ProjectLifecycleId",
            schema: "Ppm",
            table: "Projects",
            column: "ProjectLifecycleId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectLifecyclePhases_ProjectLifecycleId",
            schema: "Ppm",
            table: "ProjectLifecyclePhases",
            column: "ProjectLifecycleId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectLifecycles_State",
            schema: "Ppm",
            table: "ProjectLifecycles",
            column: "State");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectPhaseAssignments_EmployeeId",
            schema: "Ppm",
            table: "ProjectPhaseAssignments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectPhaseAssignments_ObjectId",
            schema: "Ppm",
            table: "ProjectPhaseAssignments",
            column: "ObjectId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectPhases_ProjectId",
            schema: "Ppm",
            table: "ProjectPhases",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectPhases_ProjectLifecyclePhaseId",
            schema: "Ppm",
            table: "ProjectPhases",
            column: "ProjectLifecyclePhaseId");

        migrationBuilder.AddForeignKey(
            name: "FK_Projects_ProjectLifecycles_ProjectLifecycleId",
            schema: "Ppm",
            table: "Projects",
            column: "ProjectLifecycleId",
            principalSchema: "Ppm",
            principalTable: "ProjectLifecycles",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_ProjectTasks_ProjectPhases_ProjectPhaseId",
            schema: "Ppm",
            table: "ProjectTasks",
            column: "ProjectPhaseId",
            principalSchema: "Ppm",
            principalTable: "ProjectPhases",
            principalColumn: "Id");

        // =============================================================
        // Seed 5 lifecycle templates and migrate existing project data
        // =============================================================
        var now = "SYSUTCDATETIME()";

        migrationBuilder.Sql($@"
            -- =====================================================
            -- 1. Seed Project Lifecycle templates (all Active)
            -- =====================================================

            -- 1a. Standard Waterfall Project
            DECLARE @WaterfallId UNIQUEIDENTIFIER = NEWID();
            DECLARE @WaterfallPhase1 UNIQUEIDENTIFIER = NEWID();
            DECLARE @WaterfallPhase2 UNIQUEIDENTIFIER = NEWID();
            DECLARE @WaterfallPhase3 UNIQUEIDENTIFIER = NEWID();
            DECLARE @WaterfallPhase4 UNIQUEIDENTIFIER = NEWID();
            DECLARE @WaterfallPhase5 UNIQUEIDENTIFIER = NEWID();

            INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
            VALUES (@WaterfallId, N'Standard Waterfall', N'Classic lifecycle used in traditional PMO environments. Good for infrastructure, enterprise systems, and compliance-heavy environments.', 'Active', {now}, {now});

            INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
            (@WaterfallPhase1, @WaterfallId, N'Initiation', N'Define business case and project charter', 1, {now}, {now}),
            (@WaterfallPhase2, @WaterfallId, N'Planning', N'Build detailed schedule and scope', 2, {now}, {now}),
            (@WaterfallPhase3, @WaterfallId, N'Execution', N'Build or implement the solution', 3, {now}, {now}),
            (@WaterfallPhase4, @WaterfallId, N'Monitoring & Control', N'Track progress and manage changes', 4, {now}, {now}),
            (@WaterfallPhase5, @WaterfallId, N'Closure', N'Final approvals and project close', 5, {now}, {now});

            -- 1b. Product / Software Delivery
            DECLARE @ProductId UNIQUEIDENTIFIER = NEWID();
            DECLARE @ProductPhase1 UNIQUEIDENTIFIER = NEWID();
            DECLARE @ProductPhase2 UNIQUEIDENTIFIER = NEWID();
            DECLARE @ProductPhase3 UNIQUEIDENTIFIER = NEWID();
            DECLARE @ProductPhase4 UNIQUEIDENTIFIER = NEWID();
            DECLARE @ProductPhase5 UNIQUEIDENTIFIER = NEWID();

            INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
            VALUES (@ProductId, N'Product / Software Delivery', N'Suited for modern product development. Good for SaaS, application development, and product teams.', 'Active', {now}, {now});

            INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
            (@ProductPhase1, @ProductId, N'Discovery', N'Research and validate problem', 1, {now}, {now}),
            (@ProductPhase2, @ProductId, N'Design', N'UX, architecture, solution design', 2, {now}, {now}),
            (@ProductPhase3, @ProductId, N'Build', N'Implementation and development', 3, {now}, {now}),
            (@ProductPhase4, @ProductId, N'Validate', N'Testing and quality assurance', 4, {now}, {now}),
            (@ProductPhase5, @ProductId, N'Launch', N'Release and rollout', 5, {now}, {now});

            -- 1c. SAFe / Agile Initiative
            DECLARE @SafeId UNIQUEIDENTIFIER = NEWID();
            DECLARE @SafePhase1 UNIQUEIDENTIFIER = NEWID();
            DECLARE @SafePhase2 UNIQUEIDENTIFIER = NEWID();
            DECLARE @SafePhase3 UNIQUEIDENTIFIER = NEWID();
            DECLARE @SafePhase4 UNIQUEIDENTIFIER = NEWID();
            DECLARE @SafePhase5 UNIQUEIDENTIFIER = NEWID();

            INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
            VALUES (@SafeId, N'SAFe / Agile Initiative', N'Works well when execution happens in program increments. Aligns with initiatives managed alongside SAFe.', 'Active', {now}, {now});

            INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
            (@SafePhase1, @SafeId, N'Funnel', N'Idea captured but not evaluated', 1, {now}, {now}),
            (@SafePhase2, @SafeId, N'Review', N'Initial evaluation and prioritization', 2, {now}, {now}),
            (@SafePhase3, @SafeId, N'Analysis', N'Discovery and requirements', 3, {now}, {now}),
            (@SafePhase4, @SafeId, N'Implementation', N'Development across teams', 4, {now}, {now}),
            (@SafePhase5, @SafeId, N'Release', N'Feature delivered to customers', 5, {now}, {now});

            -- 1d. Lightweight Project
            DECLARE @LightweightId UNIQUEIDENTIFIER = NEWID();
            DECLARE @LightweightPhase1 UNIQUEIDENTIFIER = NEWID();
            DECLARE @LightweightPhase2 UNIQUEIDENTIFIER = NEWID();
            DECLARE @LightweightPhase3 UNIQUEIDENTIFIER = NEWID();

            INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
            VALUES (@LightweightId, N'Lightweight Project', N'For smaller efforts where heavy process would be overkill. Good for small projects, internal initiatives, and short timelines.', 'Active', {now}, {now});

            INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
            (@LightweightPhase1, @LightweightId, N'Plan', N'Define goals and timeline', 1, {now}, {now}),
            (@LightweightPhase2, @LightweightId, N'Execute', N'Perform the work', 2, {now}, {now}),
            (@LightweightPhase3, @LightweightId, N'Deliver', N'Release or complete outcome', 3, {now}, {now});

            -- 1e. Infrastructure / IT Implementation
            DECLARE @InfraId UNIQUEIDENTIFIER = NEWID();
            DECLARE @InfraPhase1 UNIQUEIDENTIFIER = NEWID();
            DECLARE @InfraPhase2 UNIQUEIDENTIFIER = NEWID();
            DECLARE @InfraPhase3 UNIQUEIDENTIFIER = NEWID();
            DECLARE @InfraPhase4 UNIQUEIDENTIFIER = NEWID();
            DECLARE @InfraPhase5 UNIQUEIDENTIFIER = NEWID();

            INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
            VALUES (@InfraId, N'Infrastructure / IT', N'Often used for migrations, upgrades, or deployments. Common in IT PMO environments.', 'Active', {now}, {now});

            INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
            (@InfraPhase1, @InfraId, N'Assessment', N'Evaluate current environment', 1, {now}, {now}),
            (@InfraPhase2, @InfraId, N'Planning', N'Define migration or implementation approach', 2, {now}, {now}),
            (@InfraPhase3, @InfraId, N'Implementation', N'Deploy or configure systems', 3, {now}, {now}),
            (@InfraPhase4, @InfraId, N'Validation', N'Test and verify', 4, {now}, {now}),
            (@InfraPhase5, @InfraId, N'Transition', N'Hand off to operations', 5, {now}, {now});

            -- =====================================================
            -- 2. Assign Lightweight Project lifecycle to all
            --    existing projects that are Approved or beyond,
            --    plus any Proposed projects that have tasks
            -- =====================================================
            UPDATE [Ppm].[Projects]
            SET ProjectLifecycleId = @LightweightId
            WHERE [Status] != 'Proposed'
               OR Id IN (SELECT DISTINCT ProjectId FROM [Ppm].[ProjectTasks]);

            -- =====================================================
            -- 3. Create ProjectPhase instances for each assigned project
            --    (3 phases per project from Lightweight template)
            -- =====================================================
            INSERT INTO [Ppm].[ProjectPhases] (Id, ProjectId, ProjectLifecyclePhaseId, Name, Description, [Status], [Order], Progress, SystemCreated, SystemLastModified)
            SELECT NEWID(), p.Id, @LightweightPhase1, N'Plan', N'Define goals and timeline', 'NotStarted', 1, 0, {now}, {now}
            FROM [Ppm].[Projects] p
            WHERE p.ProjectLifecycleId = @LightweightId;

            INSERT INTO [Ppm].[ProjectPhases] (Id, ProjectId, ProjectLifecyclePhaseId, Name, Description, [Status], [Order], Progress, SystemCreated, SystemLastModified)
            SELECT NEWID(), p.Id, @LightweightPhase2, N'Execute', N'Perform the work', 'NotStarted', 2, 0, {now}, {now}
            FROM [Ppm].[Projects] p
            WHERE p.ProjectLifecycleId = @LightweightId;

            INSERT INTO [Ppm].[ProjectPhases] (Id, ProjectId, ProjectLifecyclePhaseId, Name, Description, [Status], [Order], Progress, SystemCreated, SystemLastModified)
            SELECT NEWID(), p.Id, @LightweightPhase3, N'Deliver', N'Release or complete outcome', 'NotStarted', 3, 0, {now}, {now}
            FROM [Ppm].[Projects] p
            WHERE p.ProjectLifecycleId = @LightweightId;

            -- =====================================================
            -- 4. Assign all root tasks to the Execute phase
            --    of their project's Lightweight lifecycle
            -- =====================================================
            UPDATE t
            SET t.ProjectPhaseId = pp.Id
            FROM [Ppm].[ProjectTasks] t
            INNER JOIN [Ppm].[Projects] p ON t.ProjectId = p.Id
            INNER JOIN [Ppm].[ProjectPhases] pp ON pp.ProjectId = p.Id AND pp.Name = N'Execute'
            WHERE p.ProjectLifecycleId = @LightweightId
              AND t.ParentId IS NULL;

            -- =====================================================
            -- 5. Propagate phase assignment to child tasks
            --    (children inherit phase from their root ancestor)
            -- =====================================================
            ;WITH TaskHierarchy AS (
                -- Root tasks: already have ProjectPhaseId set
                SELECT Id, Id AS RootId, ProjectPhaseId
                FROM [Ppm].[ProjectTasks]
                WHERE ParentId IS NULL AND ProjectPhaseId IS NOT NULL

                UNION ALL

                -- Child tasks: inherit from parent
                SELECT child.Id, parent.RootId, parent.ProjectPhaseId
                FROM [Ppm].[ProjectTasks] child
                INNER JOIN TaskHierarchy parent ON child.ParentId = parent.Id
            )
            UPDATE t
            SET t.ProjectPhaseId = th.ProjectPhaseId
            FROM [Ppm].[ProjectTasks] t
            INNER JOIN TaskHierarchy th ON t.Id = th.Id
            WHERE t.ProjectPhaseId IS NULL;
        ");

        // Make ProjectPhaseId non-nullable now that all data is populated
        migrationBuilder.AlterColumn<Guid>(
            name: "ProjectPhaseId",
            schema: "Ppm",
            table: "ProjectTasks",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Projects_ProjectLifecycles_ProjectLifecycleId",
            schema: "Ppm",
            table: "Projects");

        migrationBuilder.DropForeignKey(
            name: "FK_ProjectTasks_ProjectPhases_ProjectPhaseId",
            schema: "Ppm",
            table: "ProjectTasks");

        migrationBuilder.DropTable(
            name: "ProjectPhaseAssignments",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProjectPhases",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProjectLifecyclePhases",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProjectLifecycles",
            schema: "Ppm");

        migrationBuilder.DropIndex(
            name: "IX_ProjectTasks_ProjectPhaseId",
            schema: "Ppm",
            table: "ProjectTasks");

        migrationBuilder.DropIndex(
            name: "IX_Projects_ProjectLifecycleId",
            schema: "Ppm",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "ProjectPhaseId",
            schema: "Ppm",
            table: "ProjectTasks");

        migrationBuilder.DropColumn(
            name: "ProjectLifecycleId",
            schema: "Ppm",
            table: "Projects");
    }
}
