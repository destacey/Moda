using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddProjectTasks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Code",
            schema: "Ppm",
            table: "Projects",
            type: "varchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql(@"
            UPDATE Ppm.Projects
            SET Code = CAST([Key] AS VARCHAR(20))
            WHERE Code = '';
        ");

        migrationBuilder.CreateTable(
            name: "PpmTeams",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                Type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PpmTeams", x => x.Id);
                table.UniqueConstraint("AK_PpmTeams_Key", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "ProjectTasks",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TaskKey = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                Type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Priority = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                Order = table.Column<int>(type: "int", nullable: false),
                ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PlannedStart = table.Column<DateTime>(type: "date", nullable: true),
                PlannedEnd = table.Column<DateTime>(type: "date", nullable: true),
                ActualStart = table.Column<DateTime>(type: "date", nullable: true),
                ActualEnd = table.Column<DateTime>(type: "date", nullable: true),
                PlannedDate = table.Column<DateTime>(type: "date", nullable: true),
                ActualDate = table.Column<DateTime>(type: "date", nullable: true),
                EstimatedEffortHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                ActualEffortHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectTasks", x => x.Id);
                table.UniqueConstraint("AK_ProjectTasks_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_ProjectTasks_ProjectTasks_ParentId",
                    column: x => x.ParentId,
                    principalSchema: "Ppm",
                    principalTable: "ProjectTasks",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProjectTasks_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalSchema: "Ppm",
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProjectTaskAssignments",
            schema: "Ppm",
            columns: table => new
            {
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Role = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectTaskAssignments", x => new { x.ObjectId, x.EmployeeId, x.Role });
                table.ForeignKey(
                    name: "FK_ProjectTaskAssignments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProjectTaskAssignments_ProjectTasks_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Ppm",
                    principalTable: "ProjectTasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProjectTaskDependencies",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PredecessorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SuccessorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                RemovedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                RemovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectTaskDependencies", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProjectTaskDependencies_ProjectTasks_PredecessorId",
                    column: x => x.PredecessorId,
                    principalSchema: "Ppm",
                    principalTable: "ProjectTasks",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProjectTaskDependencies_ProjectTasks_SuccessorId",
                    column: x => x.SuccessorId,
                    principalSchema: "Ppm",
                    principalTable: "ProjectTasks",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Projects_Code",
            schema: "Ppm",
            table: "Projects",
            column: "Code",
            unique: true,
            filter: "[Code] IS NOT NULL AND [Status] <> 'Cancelled'");

        migrationBuilder.CreateIndex(
            name: "IX_PpmTeams_Code",
            schema: "Ppm",
            table: "PpmTeams",
            column: "Code",
            unique: true)
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PpmTeams_Id",
            schema: "Ppm",
            table: "PpmTeams",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_PpmTeams_IsActive",
            schema: "Ppm",
            table: "PpmTeams",
            column: "IsActive")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Code", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_PpmTeams_Key",
            schema: "Ppm",
            table: "PpmTeams",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "Type", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTaskAssignments_EmployeeId",
            schema: "Ppm",
            table: "ProjectTaskAssignments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTaskAssignments_ObjectId",
            schema: "Ppm",
            table: "ProjectTaskAssignments",
            column: "ObjectId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTaskDependencies_PredecessorId",
            schema: "Ppm",
            table: "ProjectTaskDependencies",
            column: "PredecessorId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTaskDependencies_RemovedOn",
            schema: "Ppm",
            table: "ProjectTaskDependencies",
            column: "RemovedOn");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTaskDependencies_SuccessorId",
            schema: "Ppm",
            table: "ProjectTaskDependencies",
            column: "SuccessorId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTasks_ParentId",
            schema: "Ppm",
            table: "ProjectTasks",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTasks_ProjectId",
            schema: "Ppm",
            table: "ProjectTasks",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectTasks_Status",
            schema: "Ppm",
            table: "ProjectTasks",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PpmTeams",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProjectTaskAssignments",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProjectTaskDependencies",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProjectTasks",
            schema: "Ppm");

        migrationBuilder.DropIndex(
            name: "IX_Projects_Code",
            schema: "Ppm",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "Code",
            schema: "Ppm",
            table: "Projects");
    }
}
