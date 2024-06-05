using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class Updateworktypehierarchyconfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BacklogLevels",
            schema: "Work");

        migrationBuilder.DropTable(
            name: "BacklogLevelSchemes",
            schema: "Work");

        migrationBuilder.AddColumn<int>(
            name: "LevelId",
            schema: "Work",
            table: "WorkTypes",
            type: "int",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "WorkTypeHierarchies",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkTypeHierarchies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "WorkTypeLevels",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                Tier = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Ownership = table.Column<int>(type: "int", nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                SystemCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemCreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SystemLastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                SystemLastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                WorkTypeHierarchyId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkTypeLevels", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkTypeLevels_WorkTypeHierarchies_WorkTypeHierarchyId",
                    column: x => x.WorkTypeHierarchyId,
                    principalSchema: "Work",
                    principalTable: "WorkTypeHierarchies",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypes_LevelId",
            schema: "Work",
            table: "WorkTypes",
            column: "LevelId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypeHierarchies_Id",
            schema: "Work",
            table: "WorkTypeHierarchies",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypeLevels_Id",
            schema: "Work",
            table: "WorkTypeLevels",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkTypeLevels_WorkTypeHierarchyId",
            schema: "Work",
            table: "WorkTypeLevels",
            column: "WorkTypeHierarchyId");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkTypes_WorkTypeLevels_LevelId",
            schema: "Work",
            table: "WorkTypes",
            column: "LevelId",
            principalSchema: "Work",
            principalTable: "WorkTypeLevels",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkTypes_WorkTypeLevels_LevelId",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.DropTable(
            name: "WorkTypeLevels",
            schema: "Work");

        migrationBuilder.DropTable(
            name: "WorkTypeHierarchies",
            schema: "Work");

        migrationBuilder.DropIndex(
            name: "IX_WorkTypes_LevelId",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.DropColumn(
            name: "LevelId",
            schema: "Work",
            table: "WorkTypes");

        migrationBuilder.CreateTable(
            name: "BacklogLevelSchemes",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BacklogLevelSchemes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "BacklogLevels",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Category = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Ownership = table.Column<int>(type: "int", nullable: false),
                ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Rank = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BacklogLevels", x => x.Id);
                table.ForeignKey(
                    name: "FK_BacklogLevels_BacklogLevelSchemes_ParentId",
                    column: x => x.ParentId,
                    principalSchema: "Work",
                    principalTable: "BacklogLevelSchemes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BacklogLevels_Id",
            schema: "Work",
            table: "BacklogLevels",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_BacklogLevels_ParentId",
            schema: "Work",
            table: "BacklogLevels",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_BacklogLevelSchemes_Id",
            schema: "Work",
            table: "BacklogLevelSchemes",
            column: "Id");
    }
}
