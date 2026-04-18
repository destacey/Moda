using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RenameModaUsersView : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("EXEC sp_rename 'Identity.vw_ModaUsers', 'vw_WaydUsers';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("EXEC sp_rename 'Identity.vw_WaydUsers', 'vw_ModaUsers';");
    }
}
