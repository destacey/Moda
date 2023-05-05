using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddProgramIncrement : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Planning");

        migrationBuilder.CreateTable(
            name: "ProgramIncrements",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LocalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                Start = table.Column<DateTime>(type: "date", nullable: false),
                End = table.Column<DateTime>(type: "date", nullable: false),
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
                table.PrimaryKey("PK_ProgramIncrements", x => x.Id);
                table.UniqueConstraint("AK_ProgramIncrements_LocalId", x => x.LocalId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrements_Id",
            schema: "Planning",
            table: "ProgramIncrements",
            column: "Id")
            .Annotation("SqlServer:Include", new[] { "Name", "Description" });

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrements_IsDeleted",
            schema: "Planning",
            table: "ProgramIncrements",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProgramIncrements_Start_End",
            schema: "Planning",
            table: "ProgramIncrements",
            columns: new[] { "Start", "End" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProgramIncrements",
            schema: "Planning");
    }
}
