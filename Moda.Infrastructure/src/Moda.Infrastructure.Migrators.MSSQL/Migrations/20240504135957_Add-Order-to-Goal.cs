using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddOrdertoGoal : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Objectives_Id_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_Key_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_OwnerId_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_PlanId_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.AddColumn<int>(
            name: "Order",
            schema: "Goals",
            table: "Objectives",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_Id_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Type", "Status", "OwnerId", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_Key_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Type", "Status", "OwnerId", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_OwnerId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "OwnerId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "PlanId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_PlanId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "PlanId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId", "Order" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Objectives_Id_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_Key_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_OwnerId_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropIndex(
            name: "IX_Objectives_PlanId_IsDeleted",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.DropColumn(
            name: "Order",
            schema: "Goals",
            table: "Objectives");

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_Id_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "Id", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Key", "Name", "Type", "Status", "OwnerId", "PlanId" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            column: "IsDeleted")
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId", "PlanId" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_Key_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "Key", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Name", "Type", "Status", "OwnerId", "PlanId" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_OwnerId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "OwnerId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "PlanId" });

        migrationBuilder.CreateIndex(
            name: "IX_Objectives_PlanId_IsDeleted",
            schema: "Goals",
            table: "Objectives",
            columns: new[] { "PlanId", "IsDeleted" })
            .Annotation("SqlServer:Include", new[] { "Id", "Key", "Name", "Type", "Status", "OwnerId" });
    }
}
