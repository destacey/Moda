using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddRisks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Risks",
            schema: "Planning",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LocalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Summary = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ReportedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                ReportedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Impact = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Likelihood = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                AssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                FollowUpDate = table.Column<DateTime>(type: "date", nullable: true),
                Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                table.PrimaryKey("PK_Risks", x => x.Id);
                table.UniqueConstraint("AK_Risks_LocalId", x => x.LocalId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Risks_Id",
            schema: "Planning",
            table: "Risks",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Risks_IsDeleted",
            schema: "Planning",
            table: "Risks",
            column: "IsDeleted");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Risks",
            schema: "Planning");
    }
}
