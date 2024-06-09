using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddWorkItemsExtended : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "StatusCategory",
            schema: "Work",
            table: "WorkItems",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32,
            oldNullable: true);

        migrationBuilder.CreateTable(
            name: "WorkItemsExtended",
            schema: "Work",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExternalTeamIdentifier = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkItemsExtended", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkItemsExtended_WorkItems_Id",
                    column: x => x.Id,
                    principalSchema: "Work",
                    principalTable: "WorkItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkItemsExtended_Id_ExternalTeamIdentifier",
            schema: "Work",
            table: "WorkItemsExtended",
            columns: new[] { "Id", "ExternalTeamIdentifier" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkItemsExtended",
            schema: "Work");

        migrationBuilder.AlterColumn<string>(
            name: "StatusCategory",
            schema: "Work",
            table: "WorkItems",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(32)",
            oldMaxLength: 32);
    }
}
