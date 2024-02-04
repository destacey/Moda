using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RenameWorkStatetoWorkStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkStates",
            schema: "Work");

        migrationBuilder.CreateTable(
            name: "WorkStatuses",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
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
                table.PrimaryKey("PK_WorkStatuses", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkStatuses_Id",
            schema: "Work",
            table: "WorkStatuses",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkStatuses_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkStatuses",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkStatuses_Name",
            schema: "Work",
            table: "WorkStatuses",
            column: "Name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkStatuses",
            schema: "Work");

        migrationBuilder.CreateTable(
            name: "WorkStates",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkStates", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkStates_Id",
            schema: "Work",
            table: "WorkStates",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_WorkStates_IsActive_IsDeleted",
            schema: "Work",
            table: "WorkStates",
            columns: new[] { "IsActive", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name" });

        migrationBuilder.CreateIndex(
            name: "IX_WorkStates_Name",
            schema: "Work",
            table: "WorkStates",
            column: "Name",
            unique: true);
    }
}
