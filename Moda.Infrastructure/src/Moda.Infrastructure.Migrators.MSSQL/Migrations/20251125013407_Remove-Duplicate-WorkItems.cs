using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RemoveDuplicateWorkItems : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Step 1: Delete WorkItemLinks where SourceId or TargetId references a duplicate
        migrationBuilder.Sql(@"
            WITH DuplicateRecords AS (
                SELECT 
                    wi.Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY wi.ExternalId, ws.SystemId 
                        ORDER BY wi.SystemCreated DESC
                    ) AS RowNum
                FROM [Work].[WorkItems] wi
                INNER JOIN [Work].[Workspaces] ws ON wi.WorkspaceId = ws.Id
            ),
            DuplicateIds AS (
                SELECT Id
                FROM DuplicateRecords
                WHERE RowNum > 1
            )
            DELETE FROM [Work].[WorkItemLinks]
            WHERE SourceId IN (SELECT Id FROM DuplicateIds)
               OR TargetId IN (SELECT Id FROM DuplicateIds);
            
            PRINT 'Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' work item links';
        ");

        // Step 2: Update ParentId to NULL where it references a duplicate
        migrationBuilder.Sql(@"
            WITH DuplicateRecords AS (
                SELECT 
                    wi.Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY wi.ExternalId, ws.SystemId 
                        ORDER BY wi.SystemCreated DESC
                    ) AS RowNum
                FROM [Work].[WorkItems] wi
                INNER JOIN [Work].[Workspaces] ws ON wi.WorkspaceId = ws.Id
            ),
            DuplicateIds AS (
                SELECT Id
                FROM DuplicateRecords
                WHERE RowNum > 1
            )
            UPDATE [Work].[WorkItems]
            SET ParentId = NULL
            WHERE ParentId IN (SELECT Id FROM DuplicateIds);
            
            PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' work items to clear ParentId';
        ");

        // Step 3: Delete the duplicate WorkItems
        migrationBuilder.Sql(@"
            WITH DuplicateRecords AS (
                SELECT 
                    wi.Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY wi.ExternalId, ws.SystemId 
                        ORDER BY wi.SystemCreated DESC
                    ) AS RowNum
                FROM [Work].[WorkItems] wi
                INNER JOIN [Work].[Workspaces] ws ON wi.WorkspaceId = ws.Id
            )
            DELETE FROM [Work].[WorkItems]
            WHERE Id IN (
                SELECT Id 
                FROM DuplicateRecords 
                WHERE RowNum > 1
            );
            
            PRINT 'Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' duplicate work items';
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
