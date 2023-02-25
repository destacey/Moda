using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_Code",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Id",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_LocalId",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Organization",
                table: "Teams",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Code",
                schema: "Organization",
                table: "Teams",
                column: "Code",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Id",
                schema: "Organization",
                table: "Teams",
                column: "Id")
                .Annotation("SqlServer:Include", new[] { "LocalId", "Name", "Code", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LocalId",
                schema: "Organization",
                table: "Teams",
                column: "LocalId")
                .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                schema: "Organization",
                table: "Teams",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_Code",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Id",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_LocalId",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Name",
                schema: "Organization",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Organization",
                table: "Teams",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Code",
                schema: "Organization",
                table: "Teams",
                column: "Code",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "LocalId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Id",
                schema: "Organization",
                table: "Teams",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LocalId",
                schema: "Organization",
                table: "Teams",
                column: "LocalId",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id", "Name", "Code" });
        }
    }
}
