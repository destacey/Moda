using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class DropApplicationUserObjectId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ObjectId",
            schema: "Identity",
            table: "Users");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ObjectId",
            schema: "Identity",
            table: "Users",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        // Rehydrate ObjectId from the active Entra UserIdentity row so that
        // rolling back this migration together with the PR1 code restores a
        // working lookup. Without this the old ObjectId-based lookups would
        // find no users after a rollback.
        migrationBuilder.Sql(@"
                UPDATE u
                SET u.[ObjectId] = ui.[ProviderSubject]
                FROM [Identity].[Users] u
                INNER JOIN [Identity].[UserIdentities] ui
                    ON ui.[UserId] = u.[Id]
                   AND ui.[IsActive] = 1
                   AND ui.[Provider] = 'MicrosoftEntraId';
            ");
    }
}
