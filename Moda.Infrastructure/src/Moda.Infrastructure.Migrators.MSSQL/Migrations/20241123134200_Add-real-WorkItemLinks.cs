﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddrealWorkItemLinks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WorkItemLinks",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LinkType = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EndedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                EndedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkItemLinks", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkItemLinks_Employees_CreatedById",
                    column: x => x.CreatedById,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItemLinks_Employees_EndedById",
                    column: x => x.EndedById,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItemLinks_WorkItems_SourceId",
                    column: x => x.SourceId,
                    principalSchema: "Work",
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_WorkItemLinks_WorkItems_TargetId",
                    column: x => x.TargetId,
                    principalSchema: "Work",
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemLinks_CreatedById",
            schema: "Work",
            table: "WorkItemLinks",
            column: "CreatedById");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemLinks_EndedById",
            schema: "Work",
            table: "WorkItemLinks",
            column: "EndedById");

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemLinks_SourceId_LinkType",
            schema: "Work",
            table: "WorkItemLinks",
            columns: new[] { "SourceId", "LinkType" })
            .Annotation("SqlServer:Include", new[] { "Id", "TargetId" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemLinks_TargetId_LinkType",
            schema: "Work",
            table: "WorkItemLinks",
            columns: new[] { "TargetId", "LinkType" })
            .Annotation("SqlServer:Include", new[] { "Id", "SourceId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkItemLinks",
            schema: "Work");
    }
}
