using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddStateAndHealthToDependency : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Health",
            schema: "Work",
            table: "WorkItemLinks",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "SourcePlannedOn",
            schema: "Work",
            table: "WorkItemLinks",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SourceWorkStatusCategory",
            schema: "Work",
            table: "WorkItemLinks",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "State",
            schema: "Work",
            table: "WorkItemLinks",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "TargetPlannedOn",
            schema: "Work",
            table: "WorkItemLinks",
            type: "datetime2",
            nullable: true);

        // update existing records using the two FKs to WorkItems
        migrationBuilder.Sql(@"
            UPDATE wil
            SET 
                wil.SourceWorkStatusCategory = wsrc.StatusCategory,
                wil.SourcePlannedOn = CASE WHEN wsIter.Type = 'Sprint' THEN wsIter.[End] ELSE NULL END,
                wil.TargetPlannedOn = CASE WHEN wtIter.Type = 'Sprint' THEN wtIter.[End] ELSE NULL END,
                wil.State = 
                    CASE 
                        WHEN wsrc.StatusCategory = 'Proposed' THEN 'ToDo'
                        WHEN wsrc.StatusCategory = 'Active' THEN 'InProgress'
                        WHEN wsrc.StatusCategory = 'Done' THEN 'Done'
                        WHEN wsrc.StatusCategory = 'Removed' THEN 'Removed'
                        ELSE NULL
                    END,
                wil.Health = 
                    CASE 
                        WHEN 
                            (wsrc.StatusCategory = 'Done') THEN 'Healthy'
                        WHEN 
                            (wsrc.StatusCategory = 'Removed') THEN 'Unhealthy'
                        WHEN 
                            ((CASE WHEN wsIter.Type = 'Sprint' THEN wsIter.[End] ELSE NULL END IS NULL OR CASE WHEN wsIter.Type = 'Sprint' THEN wsIter.[End] ELSE NULL END < GETUTCDATE()) AND (CASE WHEN wtIter.Type = 'Sprint' THEN wtIter.[End] ELSE NULL END IS NULL OR CASE WHEN wtIter.Type = 'Sprint' THEN wtIter.[End] ELSE NULL END < GETUTCDATE())) THEN 'AtRisk'
                        WHEN 
                            (CASE WHEN wsIter.Type = 'Sprint' THEN wsIter.[End] ELSE NULL END IS NOT NULL AND CASE WHEN wtIter.Type = 'Sprint' THEN wtIter.[End] ELSE NULL END IS NOT NULL AND CASE WHEN wsIter.Type = 'Sprint' THEN wsIter.[End] ELSE NULL END <= CASE WHEN wtIter.Type = 'Sprint' THEN wtIter.[End] ELSE NULL END) THEN 'Healthy'
                        WHEN 
                            (CASE WHEN wsIter.Type = 'Sprint' THEN wsIter.[End] ELSE NULL END IS NOT NULL AND CASE WHEN wtIter.Type = 'Sprint' THEN wtIter.[End] ELSE NULL END IS NOT NULL AND CASE WHEN wsIter.Type = 'Sprint' THEN wsIter.[End] ELSE NULL END > CASE WHEN wtIter.Type = 'Sprint' THEN wtIter.[End] ELSE NULL END) THEN 'Unhealthy'
                        WHEN 
                            (CASE WHEN wsIter.Type = 'Sprint' THEN wsIter.[End] ELSE NULL END IS NOT NULL AND CASE WHEN wtIter.Type = 'Sprint' THEN wtIter.[End] ELSE NULL END IS NULL) THEN 'Healthy'
                        ELSE 'Unknown'
                    END
            FROM Work.WorkItemLinks wil
                INNER JOIN Work.WorkItems wsrc ON wil.SourceId = wsrc.Id
                INNER JOIN Work.WorkItems wtrg ON wil.TargetId = wtrg.Id
                LEFT JOIN Work.WorkIterations wsIter ON wsrc.IterationId = wsIter.Id
                LEFT JOIN Work.WorkIterations wtIter ON wtrg.IterationId = wtIter.Id
            WHERE wil.LinkType = 'Dependency'
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Health",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "SourcePlannedOn",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "SourceWorkStatusCategory",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "State",
            schema: "Work",
            table: "WorkItemLinks");

        migrationBuilder.DropColumn(
            name: "TargetPlannedOn",
            schema: "Work",
            table: "WorkItemLinks");
    }
}
