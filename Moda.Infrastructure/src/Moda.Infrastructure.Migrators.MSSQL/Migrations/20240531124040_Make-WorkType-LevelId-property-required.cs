using Microsoft.EntityFrameworkCore.Migrations;
using Moda.Common.Domain.Enums.Work;
using Moda.Common.Domain.Enums;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class MakeWorkTypeLevelIdpropertyrequired : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        var defaultTier = WorkTypeTier.Other.ToString();
        var ownershipId = (int)Ownership.System;

        migrationBuilder.Sql($@"
            UPDATE [Work].[WorkTypes]
            SET [LevelId] = (SELECT [Id] FROM [Work].[WorkTypeLevels] WHERE [Tier] = '{defaultTier}' AND [Ownership] = {ownershipId})
            WHERE [LevelId] IS NULL");

        migrationBuilder.AlterColumn<int>(
            name: "LevelId",
            schema: "Work",
            table: "WorkTypes",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "LevelId",
            schema: "Work",
            table: "WorkTypes",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");
    }
}
