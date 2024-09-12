using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class Changeroadmaphierarchy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_Id",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_Key",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.AddColumn<int>(
            name: "Order",
            schema: "Planning",
            table: "Roadmaps",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ParentId",
            schema: "Planning",
            table: "Roadmaps",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_Id_Visibility",
            schema: "Planning",
            table: "Roadmaps",
            columns: new[] { "Id", "Visibility" })
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "ParentId" });

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_Key_Visibility",
            schema: "Planning",
            table: "Roadmaps",
            columns: new[] { "Key", "Visibility" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "ParentId" });

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_ParentId_Visibility",
            schema: "Planning",
            table: "Roadmaps",
            columns: new[] { "ParentId", "Visibility" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_Visibility",
            schema: "Planning",
            table: "Roadmaps",
            column: "Visibility")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "ParentId" });

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapManagers_RoadmapId",
            schema: "Planning",
            table: "RoadmapManagers",
            column: "RoadmapId")
            .Annotation("SqlServer:Include", new[] { "ManagerId" });

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapManagers_RoadmapId_ManagerId",
            schema: "Planning",
            table: "RoadmapManagers",
            columns: new[] { "RoadmapId", "ManagerId" });

        migrationBuilder.AddForeignKey(
            name: "FK_Roadmaps_Roadmaps_ParentId",
            schema: "Planning",
            table: "Roadmaps",
            column: "ParentId",
            principalSchema: "Planning",
            principalTable: "Roadmaps",
            principalColumn: "Id"); 
        
        migrationBuilder.Sql(@"
         IF EXISTS (SELECT 1 FROM [Planning].RoadmapLinks)
             BEGIN
                 UPDATE r
                 SET r.ParentId = rl.ParentId, r.[Order] = rl.[Order]
                 FROM Planning.Roadmaps r
                     INNER JOIN Planning.RoadmapLinks rl ON r.Id = rl.ChildId;
             END
         ");

        // Drop the RoadmapLinks table after the data has been migrated
        migrationBuilder.DropTable(
            name: "RoadmapLinks",
            schema: "Planning");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Roadmaps_Roadmaps_ParentId",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_Id_Visibility",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_Key_Visibility",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_ParentId_Visibility",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_Visibility",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropIndex(
            name: "IX_RoadmapManagers_RoadmapId",
            schema: "Planning",
            table: "RoadmapManagers");

        migrationBuilder.DropIndex(
            name: "IX_RoadmapManagers_RoadmapId_ManagerId",
            schema: "Planning",
            table: "RoadmapManagers");

        migrationBuilder.DropColumn(
            name: "Order",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropColumn(
            name: "ParentId",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.CreateTable(
            name: "RoadmapLinks",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
    }
}
