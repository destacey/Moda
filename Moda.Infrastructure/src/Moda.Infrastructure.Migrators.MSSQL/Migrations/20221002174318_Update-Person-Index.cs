using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_People_Key",
                schema: "Organization",
                table: "People");

            migrationBuilder.CreateIndex(
                name: "IX_People_Key",
                schema: "Organization",
                table: "People",
                column: "Key",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_People_Key",
                schema: "Organization",
                table: "People");

            migrationBuilder.CreateIndex(
                name: "IX_People_Key",
                schema: "Organization",
                table: "People",
                column: "Key",
                unique: true);
        }
    }
}
