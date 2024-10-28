using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class MigrateRoadmapchildrentoRoadmapItems : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
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

        migrationBuilder.CreateTable(
            name: "RoadmapItems",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoadmapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                Type = table.Column<int>(type: "int", nullable: false),
                ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Start = table.Column<DateTime>(type: "date", nullable: true),
                End = table.Column<DateTime>(type: "date", nullable: true),
                Color = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                Order = table.Column<int>(type: "int", nullable: true),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RoadmapItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_RoadmapItems_RoadmapItems_ParentId",
                    column: x => x.ParentId,
                    principalSchema: "Planning",
                    principalTable: "RoadmapItems",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_RoadmapItems_Roadmaps_RoadmapId",
                    column: x => x.RoadmapId,
                    principalSchema: "Planning",
                    principalTable: "Roadmaps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql(@"
            WITH RecursiveCTE AS (
                SELECT Id, ParentId, [Name], Description, Start, [End], Color, [Order], Id AS RootId, 0 AS Level, Created, CreatedBy, LastModified, LastModifiedBy
                FROM Planning.Roadmaps
                WHERE ParentId IS NULL
                
                UNION ALL
                
                SELECT r.Id, r.ParentId, r.[Name], r.Description, r.Start, r.[End], r.Color, r.[Order], cte.RootId, cte.Level + 1, r.Created, r.CreatedBy, r.LastModified, r.LastModifiedBy
                FROM Planning.Roadmaps r
                INNER JOIN RecursiveCTE cte ON r.ParentId = cte.Id
            )
            INSERT INTO Planning.RoadmapItems (Id, [Name], Type, Description, Start, [End], Color, ParentId, RoadmapId, [Order], SystemCreated, SystemCreatedBy, SystemLastModified, SystemLastModifiedBy)
            SELECT 
                Id, 
                [Name], 
                1,
                Description, 
                Start, 
                [End],  
                Color, 
                CASE 
                    WHEN Level = 1 THEN NULL 
                    WHEN Level > 1 THEN ParentId
                    ELSE NULL  -- This case should never occur due to the WHERE clause, but it's here for completeness
                END AS ParentId,
                RootId,
                [Order], 
                Created, CreatedBy, LastModified, LastModifiedBy
            FROM RecursiveCTE
            WHERE Level > 0
        ");

        migrationBuilder.Sql(@"
            DELETE FROM Planning.Roadmaps
            WHERE Id IN (
                SELECT Id
                FROM Planning.Roadmaps
                WHERE ParentId IS NOT NULL
            )
        ");

        migrationBuilder.DropColumn(
            name: "Color",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropColumn(
            name: "Order",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropColumn(
            name: "ParentId",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_Id_Visibility",
            schema: "Planning",
            table: "Roadmaps",
            columns: new[] { "Id", "Visibility" })
            .Annotation("SqlServer:Include", new[] { "Key", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_Key_Visibility",
            schema: "Planning",
            table: "Roadmaps",
            columns: new[] { "Key", "Visibility" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_Roadmaps_Visibility",
            schema: "Planning",
            table: "Roadmaps",
            column: "Visibility")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapItems_ParentId",
            schema: "Planning",
            table: "RoadmapItems",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_RoadmapItems_RoadmapId",
            schema: "Planning",
            table: "RoadmapItems",
            column: "RoadmapId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RoadmapItems",
            schema: "Planning");

        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_Id_Visibility",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_Key_Visibility",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.DropIndex(
            name: "IX_Roadmaps_Visibility",
            schema: "Planning",
            table: "Roadmaps");

        migrationBuilder.AddColumn<string>(
            name: "Color",
            schema: "Planning",
            table: "Roadmaps",
            type: "varchar(7)",
            maxLength: 7,
            nullable: true);

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

        migrationBuilder.AddForeignKey(
            name: "FK_Roadmaps_Roadmaps_ParentId",
            schema: "Planning",
            table: "Roadmaps",
            column: "ParentId",
            principalSchema: "Planning",
            principalTable: "Roadmaps",
            principalColumn: "Id");
    }
}
