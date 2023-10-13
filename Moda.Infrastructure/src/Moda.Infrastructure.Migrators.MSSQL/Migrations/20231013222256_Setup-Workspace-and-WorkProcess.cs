using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class SetupWorkspaceandWorkProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Work",
                table: "WorkTypes",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Work",
                table: "WorkStates",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateTable(
                name: "WorkProcesses",
                schema: "Work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Ownership = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkProcesses", x => x.Id);
                    table.UniqueConstraint("AK_WorkProcesses_Key", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                schema: "Work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Ownership = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                    table.UniqueConstraint("AK_Workspaces_Key", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Workspaces_WorkProcesses_WorkProcessId",
                        column: x => x.WorkProcessId,
                        principalSchema: "Work",
                        principalTable: "WorkProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkProcesses_Id",
                schema: "Work",
                table: "WorkProcesses",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Id",
                schema: "Work",
                table: "Workspaces",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "Name", "Ownership", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_IsActive_IsDeleted",
                schema: "Work",
                table: "Workspaces",
                columns: new[] { "IsActive", "IsDeleted" })
                .Annotation("SqlServer:Include", new[] { "Id", "Name", "Ownership" });

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Name",
                schema: "Work",
                table: "Workspaces",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_WorkProcessId",
                schema: "Work",
                table: "Workspaces",
                column: "WorkProcessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workspaces",
                schema: "Work");

            migrationBuilder.DropTable(
                name: "WorkProcesses",
                schema: "Work");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Work",
                table: "WorkTypes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Work",
                table: "WorkStates",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
