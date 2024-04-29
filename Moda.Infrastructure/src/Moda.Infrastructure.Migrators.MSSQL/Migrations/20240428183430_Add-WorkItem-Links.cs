using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkItemLinks",
                schema: "Work",
                columns: table => new
                {
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Context = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemLinks", x => new { x.WorkItemId, x.ObjectId });
                    table.ForeignKey(
                        name: "FK_WorkItemLinks_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalSchema: "Work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemLinks_ObjectId_Context",
                schema: "Work",
                table: "WorkItemLinks",
                columns: new[] { "ObjectId", "Context" })
                .Annotation("SqlServer:Include", new[] { "WorkItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemLinks_WorkItemId",
                schema: "Work",
                table: "WorkItemLinks",
                column: "WorkItemId")
                .Annotation("SqlServer:Include", new[] { "ObjectId", "Context" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkItemLinks",
                schema: "Work");
        }
    }
}
