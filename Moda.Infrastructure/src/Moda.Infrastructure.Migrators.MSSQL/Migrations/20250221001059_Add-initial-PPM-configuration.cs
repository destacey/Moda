﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddinitialPPMconfiguration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Ppm");

        migrationBuilder.CreateTable(
            name: "ExpenditureCategories",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                IsCapitalizable = table.Column<bool>(type: "bit", nullable: false),
                RequiresDepreciation = table.Column<bool>(type: "bit", nullable: false),
                AccountingCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExpenditureCategories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Portfolios",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Start = table.Column<DateTime>(type: "date", nullable: true),
                End = table.Column<DateTime>(type: "date", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Portfolios", x => x.Id);
                table.UniqueConstraint("AK_Portfolios_Key", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "StrategicThemes",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                State = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StrategicThemes", x => x.Id);
                table.UniqueConstraint("AK_StrategicThemes_Key", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "PortfolioRoleAssignments",
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
                table.PrimaryKey("PK_PortfolioRoleAssignments", x => new { x.ObjectId, x.EmployeeId, x.Role });
                table.ForeignKey(
                    name: "FK_PortfolioRoleAssignments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_PortfolioRoleAssignments_Portfolios_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Ppm",
                    principalTable: "Portfolios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Programs",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Start = table.Column<DateTime>(type: "date", nullable: true),
                End = table.Column<DateTime>(type: "date", nullable: true),
                PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Programs", x => x.Id);
                table.UniqueConstraint("AK_Programs_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_Programs_Portfolios_PortfolioId",
                    column: x => x.PortfolioId,
                    principalSchema: "Ppm",
                    principalTable: "Portfolios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProgramRoleAssignments",
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
                table.PrimaryKey("PK_ProgramRoleAssignments", x => new { x.ObjectId, x.EmployeeId, x.Role });
                table.ForeignKey(
                    name: "FK_ProgramRoleAssignments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProgramRoleAssignments_Programs_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Ppm",
                    principalTable: "Programs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProgramStrategicThemes",
            schema: "Ppm",
            columns: table => new
            {
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StrategicThemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProgramStrategicThemes", x => new { x.ObjectId, x.StrategicThemeId });
                table.ForeignKey(
                    name: "FK_ProgramStrategicThemes_Programs_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Ppm",
                    principalTable: "Programs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProgramStrategicThemes_StrategicThemes_StrategicThemeId",
                    column: x => x.StrategicThemeId,
                    principalSchema: "Ppm",
                    principalTable: "StrategicThemes",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "Projects",
            schema: "Ppm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                Start = table.Column<DateTime>(type: "date", nullable: true),
                End = table.Column<DateTime>(type: "date", nullable: true),
                ExpenditureCategoryId = table.Column<int>(type: "int", nullable: false),
                PortfolioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Projects", x => x.Id);
                table.UniqueConstraint("AK_Projects_Key", x => x.Key);
                table.ForeignKey(
                    name: "FK_Projects_ExpenditureCategories_ExpenditureCategoryId",
                    column: x => x.ExpenditureCategoryId,
                    principalSchema: "Ppm",
                    principalTable: "ExpenditureCategories",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Projects_Portfolios_PortfolioId",
                    column: x => x.PortfolioId,
                    principalSchema: "Ppm",
                    principalTable: "Portfolios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Projects_Programs_ProgramId",
                    column: x => x.ProgramId,
                    principalSchema: "Ppm",
                    principalTable: "Programs",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "ProjectRoleAssignments",
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
                table.PrimaryKey("PK_ProjectRoleAssignments", x => new { x.ObjectId, x.EmployeeId, x.Role });
                table.ForeignKey(
                    name: "FK_ProjectRoleAssignments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ProjectRoleAssignments_Projects_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Ppm",
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProjectStrategicThemes",
            schema: "Ppm",
            columns: table => new
            {
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StrategicThemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectStrategicThemes", x => new { x.ObjectId, x.StrategicThemeId });
                table.ForeignKey(
                    name: "FK_ProjectStrategicThemes_Projects_ObjectId",
                    column: x => x.ObjectId,
                    principalSchema: "Ppm",
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProjectStrategicThemes_StrategicThemes_StrategicThemeId",
                    column: x => x.StrategicThemeId,
                    principalSchema: "Ppm",
                    principalTable: "StrategicThemes",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_PortfolioRoleAssignments_EmployeeId",
            schema: "Ppm",
            table: "PortfolioRoleAssignments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_PortfolioRoleAssignments_ObjectId",
            schema: "Ppm",
            table: "PortfolioRoleAssignments",
            column: "ObjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Portfolios_Status",
            schema: "Ppm",
            table: "Portfolios",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramRoleAssignments_EmployeeId",
            schema: "Ppm",
            table: "ProgramRoleAssignments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramRoleAssignments_ObjectId",
            schema: "Ppm",
            table: "ProgramRoleAssignments",
            column: "ObjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Programs_PortfolioId",
            schema: "Ppm",
            table: "Programs",
            column: "PortfolioId");

        migrationBuilder.CreateIndex(
            name: "IX_Programs_Status",
            schema: "Ppm",
            table: "Programs",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramStrategicThemes_ObjectId",
            schema: "Ppm",
            table: "ProgramStrategicThemes",
            column: "ObjectId");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramStrategicThemes_StrategicThemeId",
            schema: "Ppm",
            table: "ProgramStrategicThemes",
            column: "StrategicThemeId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectRoleAssignments_EmployeeId",
            schema: "Ppm",
            table: "ProjectRoleAssignments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectRoleAssignments_ObjectId",
            schema: "Ppm",
            table: "ProjectRoleAssignments",
            column: "ObjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_ExpenditureCategoryId",
            schema: "Ppm",
            table: "Projects",
            column: "ExpenditureCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_PortfolioId",
            schema: "Ppm",
            table: "Projects",
            column: "PortfolioId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_ProgramId",
            schema: "Ppm",
            table: "Projects",
            column: "ProgramId");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_Status",
            schema: "Ppm",
            table: "Projects",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectStrategicThemes_ObjectId",
            schema: "Ppm",
            table: "ProjectStrategicThemes",
            column: "ObjectId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectStrategicThemes_StrategicThemeId",
            schema: "Ppm",
            table: "ProjectStrategicThemes",
            column: "StrategicThemeId");

        migrationBuilder.CreateIndex(
            name: "IX_StrategicThemes_State",
            schema: "Ppm",
            table: "StrategicThemes",
            column: "State");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PortfolioRoleAssignments",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProgramRoleAssignments",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProgramStrategicThemes",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProjectRoleAssignments",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ProjectStrategicThemes",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "Projects",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "StrategicThemes",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "ExpenditureCategories",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "Programs",
            schema: "Ppm");

        migrationBuilder.DropTable(
            name: "Portfolios",
            schema: "Ppm");
    }
}
