using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddRoadmaps : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_TeamMemberships_SourceId",
            schema: "Organization",
            table: "TeamMemberships");

        migrationBuilder.DropIndex(
            name: "IX_TeamMemberships_TargetId",
            schema: "Organization",
            table: "TeamMemberships");

        migrationBuilder.CreateTable(
            name: "Roadmaps",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Key = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                Visibility = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                End = table.Column<DateTime>(type: "date", nullable: false),
                Start = table.Column<DateTime>(type: "date", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roadmaps", x => x.Id);
                table.UniqueConstraint("AK_Roadmaps_Key", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "RoadmapLinks",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoadmapLinks", x => x.Id);
                table.ForeignKey(
                    name: "FK_RoadmapLinks_Roadmaps_ChildId",
                    column: x => x.ChildId,
                    principalSchema: "Planning",
                    principalTable: "Roadmaps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RoadmapLinks_Roadmaps_ParentId",
                    column: x => x.ParentId,
                    principalSchema: "Planning",
                    principalTable: "Roadmaps",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "RoadmapManagers",
            schema: "Planning",
            columns: table => new
            {
                RoadmapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoadmapManagers", x => new { x.RoadmapId, x.ManagerId });
                table.ForeignKey(
                    name: "FK_RoadmapManagers_Employees_ManagerId",
                    column: x => x.ManagerId,
                    principalSchema: "Organization",
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_RoadmapManagers_Roadmaps_RoadmapId",
                    column: x => x.RoadmapId,
                    principalSchema: "Planning",
                    principalTable: "Roadmaps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberships_SourceId_IsDeleted",
            schema: "Organization",
            table: "TeamMemberships",
            columns: new[] { "SourceId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "TargetId" });

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberships_TargetId_IsDeleted",
            schema: "Organization",
            table: "TeamMemberships",
            columns: new[] { "TargetId", "IsDeleted" },
            filter: "[IsDeleted] = 0")
            .Annotation("SqlServer:Include", new[] { "Id", "SourceId" });

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapLinks_ChildId",
            schema: "Planning",
            table: "RoadmapLinks",
            column: "ChildId")
            .Annotation("SqlServer:Include", new[] { "Id", "ParentId" });

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapLinks_Id",
            schema: "Planning",
            table: "RoadmapLinks",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "ParentId", "ChildId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapLinks_ParentId",
            schema: "Planning",
            table: "RoadmapLinks",
            column: "ParentId")
            .Annotation("SqlServer:Include", new[] { "Id", "ChildId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapManagers_ManagerId",
            schema: "Planning",
            table: "RoadmapManagers",
            column: "ManagerId");

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_Id",
            schema: "Planning",
            table: "Roadmaps",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Visibility" });

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_Key",
            schema: "Planning",
            table: "Roadmaps",
            column: "Key")
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Visibility" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RoadmapLinks",
            schema: "Planning");

        migrationBuilder.DropTable(
            name: "RoadmapManagers",
            schema: "Planning");

        migrationBuilder.DropTable(
            name: "Roadmaps",
            schema: "Planning");

        migrationBuilder.DropIndex(
            name: "IX_TeamMemberships_SourceId_IsDeleted",
            schema: "Organization",
            table: "TeamMemberships");

        migrationBuilder.DropIndex(
            name: "IX_TeamMemberships_TargetId_IsDeleted",
            schema: "Organization",
            table: "TeamMemberships");

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberships_SourceId",
            schema: "Organization",
            table: "TeamMemberships",
            column: "SourceId");

        migrationBuilder.CreateIndex(
            name: "IX_TeamMemberships_TargetId",
            schema: "Organization",
            table: "TeamMemberships",
            column: "TargetId");
    }
}
