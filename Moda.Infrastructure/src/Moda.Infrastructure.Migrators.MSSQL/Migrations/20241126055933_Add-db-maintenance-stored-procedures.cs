using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class Adddbmaintenancestoredprocedures : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // First create the index maintenance procedure (unchanged)
        migrationBuilder.Sql(@"
            CREATE OR ALTER PROCEDURE [dbo].[IndexMaintenance]
                @MinFragmentation FLOAT = 5.0,
                @RebuildThreshold FLOAT = 30.0,
                @MinPageCount INT = 1000,
                @MaxDOP INT = 4,
                @TableName NVARCHAR(128) = NULL
            AS
            BEGIN
                SET NOCOUNT ON;
    
                DECLARE @SQL NVARCHAR(MAX);
                DECLARE @SchemaName NVARCHAR(128);
                DECLARE @CurrentTableName NVARCHAR(128);
                DECLARE @IndexName NVARCHAR(128);
                DECLARE @AvgFragmentation FLOAT;
                DECLARE @PageCount INT;
                DECLARE @StartTime DATETIME;
                DECLARE @EndTime DATETIME;
                DECLARE @ErrorMessage NVARCHAR(4000);
    
                CREATE TABLE #FragmentedIndexes
                (
                    SchemaName NVARCHAR(128),
                    TableName NVARCHAR(128),
                    IndexName NVARCHAR(128),
                    AvgFragmentation FLOAT,
                    PageCount INT,
                    StartTime DATETIME NULL,
                    EndTime DATETIME NULL,
                    [Status] NVARCHAR(100) DEFAULT 'Pending'
                );

                INSERT INTO #FragmentedIndexes
                    (SchemaName, TableName, IndexName, AvgFragmentation, PageCount)
                SELECT 
                    SCHEMA_NAME(t.schema_id) AS SchemaName,
                    t.name AS TableName,
                    i.name AS IndexName,
                    stats.avg_fragmentation_in_percent AS AvgFragmentation,
                    stats.page_count AS PageCount
                FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') stats
                INNER JOIN sys.tables t ON stats.object_id = t.object_id
                INNER JOIN sys.indexes i ON stats.object_id = i.object_id 
                    AND stats.index_id = i.index_id
                WHERE 
                    stats.index_id > 0
                    AND stats.page_count > @MinPageCount
                    AND stats.avg_fragmentation_in_percent > @MinFragmentation
                    AND (@TableName IS NULL OR t.name = @TableName)
                    AND i.name IS NOT NULL;

                DECLARE IndexCursor CURSOR FOR
                SELECT SchemaName, TableName, IndexName, AvgFragmentation, PageCount
                FROM #FragmentedIndexes
                ORDER BY AvgFragmentation DESC, PageCount DESC;

                OPEN IndexCursor;
                FETCH NEXT FROM IndexCursor INTO @SchemaName, @CurrentTableName, @IndexName, @AvgFragmentation, @PageCount;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SET @StartTime = GETDATE();
        
                    UPDATE #FragmentedIndexes 
                    SET StartTime = @StartTime,
                        [Status] = 'In Progress'
                    WHERE SchemaName = @SchemaName 
                        AND TableName = @CurrentTableName
                        AND IndexName = @IndexName;

                    BEGIN TRY
                        IF @AvgFragmentation > @RebuildThreshold AND @PageCount > @MinPageCount
                        BEGIN
                            SET @SQL = N'ALTER INDEX ' + QUOTENAME(@IndexName) + 
                                      N' ON ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@CurrentTableName) + 
                                      N' REBUILD WITH (ONLINE = ON, MAXDOP = ' + CAST(@MaxDOP AS NVARCHAR(2)) + ')';
                        END
                        ELSE
                        BEGIN
                            SET @SQL = N'ALTER INDEX ' + QUOTENAME(@IndexName) + 
                                      N' ON ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@CurrentTableName) + 
                                      N' REORGANIZE';
                        END

                        EXEC sp_executesql @SQL;
            
                        SET @EndTime = GETDATE();
            
                        UPDATE #FragmentedIndexes 
                        SET EndTime = @EndTime,
                            [Status] = 'Completed'
                        WHERE SchemaName = @SchemaName 
                            AND TableName = @CurrentTableName
                            AND IndexName = @IndexName;
                    END TRY
                    BEGIN CATCH
                        SET @ErrorMessage = ERROR_MESSAGE();
            
                        UPDATE #FragmentedIndexes 
                        SET EndTime = GETDATE(),
                            [Status] = 'Error: ' + @ErrorMessage
                        WHERE SchemaName = @SchemaName 
                            AND TableName = @CurrentTableName
                            AND IndexName = @IndexName;
                    END CATCH

                    FETCH NEXT FROM IndexCursor INTO @SchemaName, @CurrentTableName, @IndexName, @AvgFragmentation, @PageCount;
                END

                CLOSE IndexCursor;
                DEALLOCATE IndexCursor;

                SELECT 
                    SchemaName,
                    TableName,
                    IndexName,
                    AvgFragmentation,
                    PageCount,
                    [Status],
                    StartTime,
                    EndTime,
                    CASE 
                        WHEN EndTime IS NOT NULL THEN 
                            DATEDIFF(SECOND, StartTime, EndTime)
                        ELSE NULL
                    END AS DurationSeconds
                FROM #FragmentedIndexes
                ORDER BY 
                    CASE 
                        WHEN [Status] = 'Error' THEN 1
                        WHEN [Status] = 'In Progress' THEN 2
                        WHEN [Status] = 'Completed' THEN 3
                        ELSE 4
                    END,
                    AvgFragmentation DESC;

                DROP TABLE #FragmentedIndexes;
            END;");

        // Then create the statistics update procedure
        migrationBuilder.Sql(@"
            CREATE OR ALTER PROCEDURE [dbo].[UpdateStatistics]
                @TableName NVARCHAR(128) = NULL,
                @UseSamplePercent BIT = 0,
                @SamplePercent INT = 50,
                @MaxDOP INT = 4,
                @TimeLimit INT = NULL,
                @LowImpactMode BIT = 0
            AS
            BEGIN
                SET NOCOUNT ON;

                DECLARE @StartTime DATETIME = GETDATE();
                DECLARE @CurrentTableName NVARCHAR(500);
                DECLARE @SchemaName NVARCHAR(128);
                DECLARE @TableStartTime DATETIME;
                DECLARE @SQL NVARCHAR(1000);
                DECLARE @ErrorMessage NVARCHAR(4000);
                DECLARE @StatsName NVARCHAR(128);
                DECLARE @TotalRows BIGINT;
    
                CREATE TABLE #StatisticsUpdateLog
                (
                    SchemaName NVARCHAR(128),
                    TableName NVARCHAR(128),
                    StatisticsName NVARCHAR(128),
                    StartTime DATETIME,
                    EndTime DATETIME NULL,
                    [Status] NVARCHAR(100) DEFAULT 'Pending',
                    TotalRows BIGINT NULL,
                    Error NVARCHAR(4000) NULL,
                    Duration AS DATEDIFF(SECOND, StartTime, ISNULL(EndTime, GETDATE()))
                );

                INSERT INTO #StatisticsUpdateLog (SchemaName, TableName, StatisticsName, TotalRows)
                SELECT 
                    OBJECT_SCHEMA_NAME(t.object_id) AS SchemaName,
                    t.name AS TableName,
                    s.name AS StatisticsName,
                    p.rows AS TotalRows
                FROM sys.tables t
                INNER JOIN sys.stats s ON t.object_id = s.object_id
                INNER JOIN sys.partitions p ON t.object_id = p.object_id
                    AND p.index_id IN (0,1)
                WHERE 
                    t.is_ms_shipped = 0
                    AND (@TableName IS NULL OR t.name = @TableName)
                ORDER BY p.rows DESC;

                -- Fixed cursor declaration to include TotalRows in the SELECT DISTINCT
                DECLARE StatsCursor CURSOR FOR
                SELECT DISTINCT SchemaName, TableName, TotalRows
                FROM #StatisticsUpdateLog
                ORDER BY TotalRows DESC;

                OPEN StatsCursor;
                FETCH NEXT FROM StatsCursor INTO @SchemaName, @CurrentTableName, @TotalRows;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF @TimeLimit IS NOT NULL AND DATEDIFF(MINUTE, @StartTime, GETDATE()) > @TimeLimit
                    BEGIN
                        UPDATE #StatisticsUpdateLog
                        SET [Status] = 'Skipped - Time Limit Reached'
                        WHERE [Status] = 'Pending';
                        BREAK;
                    END

                    DECLARE StatisticsCursor CURSOR FOR
                    SELECT StatisticsName
                    FROM #StatisticsUpdateLog
                    WHERE SchemaName = @SchemaName 
                        AND TableName = @CurrentTableName
                        AND [Status] = 'Pending';

                    OPEN StatisticsCursor;
                    FETCH NEXT FROM StatisticsCursor INTO @StatsName;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        SET @TableStartTime = GETDATE();

                        UPDATE #StatisticsUpdateLog
                        SET StartTime = @TableStartTime,
                            [Status] = 'In Progress'
                        WHERE SchemaName = @SchemaName 
                            AND TableName = @CurrentTableName
                            AND StatisticsName = @StatsName;

                        BEGIN TRY
                            SET @SQL = N'UPDATE STATISTICS ' + 
                                      QUOTENAME(@SchemaName) + '.' + QUOTENAME(@CurrentTableName) + 
                                      '(' + QUOTENAME(@StatsName) + ')' +
                                      CASE 
                                          WHEN @UseSamplePercent = 1 
                                          THEN ' WITH SAMPLE ' + CAST(@SamplePercent AS NVARCHAR(3)) + ' PERCENT'
                                          ELSE ' WITH FULLSCAN'
                                      END +
                                      ', MAXDOP = ' + CAST(@MaxDOP AS NVARCHAR(2));

                            EXEC sp_executesql @SQL;

                            UPDATE #StatisticsUpdateLog
                            SET EndTime = GETDATE(),
                                [Status] = 'Completed'
                            WHERE SchemaName = @SchemaName 
                                AND TableName = @CurrentTableName
                                AND StatisticsName = @StatsName;

                        END TRY
                        BEGIN CATCH
                            SET @ErrorMessage = ERROR_MESSAGE();
                
                            UPDATE #StatisticsUpdateLog
                            SET EndTime = GETDATE(),
                                [Status] = 'Error',
                                Error = @ErrorMessage
                            WHERE SchemaName = @SchemaName 
                                AND TableName = @CurrentTableName
                                AND StatisticsName = @StatsName;
                        END CATCH

                        IF @LowImpactMode = 1
                            WAITFOR DELAY '00:00:01';

                        FETCH NEXT FROM StatisticsCursor INTO @StatsName;
                    END

                    CLOSE StatisticsCursor;
                    DEALLOCATE StatisticsCursor;

                    FETCH NEXT FROM StatsCursor INTO @SchemaName, @CurrentTableName, @TotalRows;
                END

                CLOSE StatsCursor;
                DEALLOCATE StatsCursor;

                SELECT 
                    SchemaName,
                    TableName,
                    StatisticsName,
                    [Status],
                    StartTime,
                    EndTime,
                    Duration as DurationSeconds,
                    TotalRows,
                    Error
                FROM #StatisticsUpdateLog
                ORDER BY 
                    CASE [Status]
                        WHEN 'Error' THEN 1
                        WHEN 'In Progress' THEN 2
                        WHEN 'Completed' THEN 3
                        WHEN 'Skipped - Time Limit Reached' THEN 4
                        WHEN 'Pending' THEN 5
                    END,
                    TotalRows DESC;

                DROP TABLE #StatisticsUpdateLog;
            END;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[IndexMaintenance]");
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[UpdateStatistics]");
    }
}
