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

        // =============================================================
        // Seed lifecycle templates and migrate existing project data
        // =============================================================
        var now = "SYSUTCDATETIME()";

        migrationBuilder.Sql($@"
			-- =====================================================
			-- 1. Seed Project Lifecycle templates (all Active)
			-- =====================================================

			-- 1a. Standard Project
			DECLARE @StandardId UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0001';
			DECLARE @StandardPhase1 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0101';
			DECLARE @StandardPhase2 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0102';
			DECLARE @StandardPhase3 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0103';
			DECLARE @StandardPhase4 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0104';
			DECLARE @StandardPhase5 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0105';

			INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
			VALUES (@StandardId, N'Standard Project',
			N'A traditional project lifecycle designed for initiatives that require structured planning, defined scope, and formal oversight. This template progresses from initiation through planning, execution, monitoring, and closure.',
			'Proposed', {now}, {now});

			INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
			(@StandardPhase1, @StandardId, N'Initiation', N'Define the business case, objectives, and project charter.', 1, {now}, {now}),
			(@StandardPhase2, @StandardId, N'Planning', N'Develop the detailed scope, schedule, and delivery plan.', 2, {now}, {now}),
			(@StandardPhase3, @StandardId, N'Execution', N'Execute the project plan and produce the planned deliverables.', 3, {now}, {now}),
			(@StandardPhase4, @StandardId, N'Monitoring & Control', N'Track progress, manage risks and changes, and ensure alignment with the project plan.', 4, {now}, {now}),
			(@StandardPhase5, @StandardId, N'Closure', N'Complete final deliverables, obtain approvals, and formally close the project.', 5, {now}, {now});

			-- 1b. Software Delivery
			DECLARE @SoftwareId UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0002';
			DECLARE @SoftwarePhase1 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0201';
			DECLARE @SoftwarePhase2 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0202';
			DECLARE @SoftwarePhase3 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0203';
			DECLARE @SoftwarePhase4 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0204';
			DECLARE @SoftwarePhase5 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0205';

			INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
			VALUES (@SoftwareId, N'Software Delivery',
			N'A lifecycle tailored for delivering software products or digital solutions. It emphasizes discovery, solution design, iterative development, validation, and release.',
			'Proposed', {now}, {now});

			INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
			(@SoftwarePhase1, @SoftwareId, N'Discovery', N'Research the problem space and validate requirements.', 1, {now}, {now}),
			(@SoftwarePhase2, @SoftwareId, N'Design', N'Define architecture, user experience, and technical approach.', 2, {now}, {now}),
			(@SoftwarePhase3, @SoftwareId, N'Build', N'Develop and integrate the software solution.', 3, {now}, {now}),
			(@SoftwarePhase4, @SoftwareId, N'Validate', N'Test the solution and ensure it meets quality and functional expectations.', 4, {now}, {now}),
			(@SoftwarePhase5, @SoftwareId, N'Launch', N'Deploy the solution and release it to users or customers.', 5, {now}, {now});

			-- 1c. Agile Initiative
			DECLARE @AgileId UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0003';
			DECLARE @AgilePhase1 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0301';
			DECLARE @AgilePhase2 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0302';
			DECLARE @AgilePhase3 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0303';
			DECLARE @AgilePhase4 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0304';
			DECLARE @AgilePhase5 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0305';

			INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
			VALUES (@AgileId, N'Agile Initiative',
			N'A lightweight lifecycle for initiatives delivered primarily through Agile teams. Planning occurs at a high level while detailed execution is managed within team backlogs and iterations.',
			'Proposed', {now}, {now});

			INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
			(@AgilePhase1, @AgileId, N'Funnel', N'Capture the idea or opportunity for consideration.', 1, {now}, {now}),
			(@AgilePhase2, @AgileId, N'Review', N'Evaluate and prioritize the initiative.', 2, {now}, {now}),
			(@AgilePhase3, @AgileId, N'Analysis', N'Perform discovery and define high-level requirements.', 3, {now}, {now}),
			(@AgilePhase4, @AgileId, N'Implementation', N'Execute delivery across Agile teams and iterations.', 4, {now}, {now}),
			(@AgilePhase5, @AgileId, N'Release', N'Deliver completed features or capabilities to customers.', 5, {now}, {now});

			-- 1d. Lightweight Project
			DECLARE @LightweightId UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0004';
			DECLARE @LightweightPhase1 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0401';
			DECLARE @LightweightPhase2 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0402';
			DECLARE @LightweightPhase3 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0403';

			INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
			VALUES (@LightweightId, N'Lightweight Project',
			N'A simplified project lifecycle for smaller initiatives that do not require extensive governance or complex planning.',
			'Active', {now}, {now});

			INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
			(@LightweightPhase1, @LightweightId, N'Plan', N'Define objectives, scope, and a basic timeline.', 1, {now}, {now}),
			(@LightweightPhase2, @LightweightId, N'Execute', N'Carry out the work required to achieve the objectives.', 2, {now}, {now}),
			(@LightweightPhase3, @LightweightId, N'Deliver', N'Finalize outcomes and deliver results.', 3, {now}, {now});

			-- 1e. Infrastructure Implementation
			DECLARE @InfraId UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0005';
			DECLARE @InfraPhase1 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0501';
			DECLARE @InfraPhase2 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0502';
			DECLARE @InfraPhase3 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0503';
			DECLARE @InfraPhase4 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0504';
			DECLARE @InfraPhase5 UNIQUEIDENTIFIER = '0195a1d0-7a0a-7f3a-a4e1-6b8f1c2b0505';

			INSERT INTO [Ppm].[ProjectLifecycles] (Id, Name, Description, [State], SystemCreated, SystemLastModified)
			VALUES (@InfraId, N'Infrastructure Implementation',
			N'A lifecycle designed for deploying or upgrading infrastructure and technical platforms, including planning, deployment, validation, and transition to operations.',
			'Proposed', {now}, {now});

			INSERT INTO [Ppm].[ProjectLifecyclePhases] (Id, ProjectLifecycleId, Name, Description, [Order], SystemCreated, SystemLastModified) VALUES
			(@InfraPhase1, @InfraId, N'Assessment', N'Evaluate the current environment and requirements.', 1, {now}, {now}),
			(@InfraPhase2, @InfraId, N'Planning', N'Define the implementation or migration strategy.', 2, {now}, {now}),
			(@InfraPhase3, @InfraId, N'Implementation', N'Deploy or configure infrastructure and systems.', 3, {now}, {now}),
			(@InfraPhase4, @InfraId, N'Validation', N'Confirm the solution functions as expected.', 4, {now}, {now}),
			(@InfraPhase5, @InfraId, N'Transition', N'Hand off the solution to operational teams.', 5, {now}, {now});

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
			SELECT NEWID(), p.Id, @LightweightPhase1, N'Plan', N'Define objectives, scope, and a basic timeline.', 'NotStarted', 1, 0, {now}, {now}
			FROM [Ppm].[Projects] p
			WHERE p.ProjectLifecycleId = @LightweightId;

			INSERT INTO [Ppm].[ProjectPhases] (Id, ProjectId, ProjectLifecyclePhaseId, Name, Description, [Status], [Order], Progress, SystemCreated, SystemLastModified)
			SELECT NEWID(), p.Id, @LightweightPhase2, N'Execute', N'Carry out the work required to achieve the objectives.', 'NotStarted', 2, 0, {now}, {now}
			FROM [Ppm].[Projects] p
			WHERE p.ProjectLifecycleId = @LightweightId;

			INSERT INTO [Ppm].[ProjectPhases] (Id, ProjectId, ProjectLifecyclePhaseId, Name, Description, [Status], [Order], Progress, SystemCreated, SystemLastModified)
			SELECT NEWID(), p.Id, @LightweightPhase3, N'Deliver', N'Finalize outcomes and deliver results.', 'NotStarted', 3, 0, {now}, {now}
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
				SELECT Id, Id AS RootId, ProjectPhaseId
				FROM [Ppm].[ProjectTasks]
				WHERE ParentId IS NULL AND ProjectPhaseId IS NOT NULL

				UNION ALL

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
