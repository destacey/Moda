using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddUserIdentitiesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserIdentities",
            schema: "Identity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ProviderTenantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ProviderSubject = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UnlinkedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UnlinkReason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserIdentities", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserIdentities_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "Identity",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserIdentities_UserId",
            schema: "Identity",
            table: "UserIdentities",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "UX_UserIdentities_Provider_Tenant_Subject_Active",
            schema: "Identity",
            table: "UserIdentities",
            columns: new[] { "Provider", "ProviderTenantId", "ProviderSubject" },
            unique: true,
            filter: "[IsActive] = 1");

        // Backfill: one UserIdentity row per existing ApplicationUser.
        //   - Entra users (LoginProvider = 'MicrosoftEntraId' AND ObjectId IS NOT NULL)
        //     get a row keyed by ObjectId (ProviderTenantId is left NULL; the next
        //     login populates it via the null-tid upgrade path in UserService).
        //   - Local ('Wayd') users get a row keyed by ApplicationUser.Id — the stable
        //     identifier. Usernames are mutable; ApplicationUser.Id is not.
        // NEWID() is acceptable here because each row is inserted once and the PK is
        // never referenced by FK (UserId is the link back to the user).
        migrationBuilder.Sql(@"
                INSERT INTO [Identity].[UserIdentities]
                    ([Id], [UserId], [Provider], [ProviderTenantId], [ProviderSubject],
                     [IsActive], [LinkedAt], [UnlinkedAt], [UnlinkReason])
                SELECT NEWID(), [Id], 'MicrosoftEntraId', NULL, [ObjectId],
                       1, SYSUTCDATETIME(), NULL, NULL
                FROM [Identity].[Users]
                WHERE [LoginProvider] = 'MicrosoftEntraId' AND [ObjectId] IS NOT NULL;

                INSERT INTO [Identity].[UserIdentities]
                    ([Id], [UserId], [Provider], [ProviderTenantId], [ProviderSubject],
                     [IsActive], [LinkedAt], [UnlinkedAt], [UnlinkReason])
                SELECT NEWID(), [Id], 'Wayd', NULL, [Id],
                       1, SYSUTCDATETIME(), NULL, NULL
                FROM [Identity].[Users]
                WHERE [LoginProvider] = 'Wayd';
            ");

        // Post-migration assertion: every user who can currently authenticate must
        // have a matching active UserIdentity row. We deliberately exclude two
        // legitimate pre-state cases so the migration doesn't fail on them:
        //   - Entra users who have never logged in (ObjectId IS NULL): the row is
        //     written on their first SSO login via the EnsureEntraIdentityRow path.
        //   - Users with an unrecognized LoginProvider value: these are data-quality
        //     outliers that should be investigated separately, not blocking here.
        migrationBuilder.Sql(@"
                DECLARE @Missing int;
                SELECT @Missing = COUNT(*)
                FROM [Identity].[Users] u
                LEFT JOIN [Identity].[UserIdentities] ui
                    ON ui.[UserId] = u.[Id] AND ui.[IsActive] = 1
                WHERE ui.[Id] IS NULL
                  AND (
                        (u.[LoginProvider] = 'MicrosoftEntraId' AND u.[ObjectId] IS NOT NULL)
                     OR (u.[LoginProvider] = 'Wayd')
                      );

                IF @Missing > 0
                BEGIN
                    DECLARE @Msg nvarchar(200) = CONCAT(
                        'UserIdentity backfill incomplete: ', @Missing,
                        ' authenticable ApplicationUser row(s) have no active UserIdentity. ',
                        'Investigate before proceeding.');
                    THROW 50000, @Msg, 1;
                END
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserIdentities",
            schema: "Identity");
    }
}
