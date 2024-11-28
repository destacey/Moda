using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddTeamDatesAndGraphTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
           name: "ActiveDate",
           schema: "Organization",
           table: "Teams",
           type: "date",
           nullable: false,
           defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<DateTime>(
            name: "InactiveDate",
            schema: "Organization",
            table: "Teams",
            type: "date",
            nullable: true);

        migrationBuilder.Sql(@"
            -- Set ActiveDate based on earliest of Created or membership dates
            UPDATE t
            SET t.ActiveDate = CAST(
                CASE 
                    WHEN m.EarliestStart IS NULL THEN t.Created
                    WHEN m.EarliestStart < t.Created THEN m.EarliestStart
                    ELSE t.Created
                END as date)
            FROM [Organization].[Teams] t
            LEFT JOIN (
                SELECT 
                    TeamId,
                    MIN(EarliestStart) as EarliestStart
                FROM (
                    -- Get earliest dates as a source team
                    SELECT SourceId as TeamId, MIN([Start]) as EarliestStart
                    FROM [Organization].[TeamMemberships]
                    WHERE IsDeleted = 0
                    GROUP BY SourceId
                
                    UNION
                
                    -- Get earliest dates as a target team
                    SELECT TargetId as TeamId, MIN([Start]) as EarliestStart
                    FROM [Organization].[TeamMemberships]
                    WHERE IsDeleted = 0
                    GROUP BY TargetId
                ) x
                GROUP BY TeamId
            ) m ON t.Id = m.TeamId;

            -- Set InactiveDate for inactive teams
            WITH LatestMemberships AS (
                SELECT 
                    TeamId,
                    MAX(EndDate) as LatestEnd
                FROM (
                    -- Get latest end dates as a source team
                    SELECT 
                        SourceId as TeamId, 
                        MAX([End]) as EndDate
                    FROM [Organization].[TeamMemberships]
                    WHERE IsDeleted = 0
                    GROUP BY SourceId
                
                    UNION
                
                    -- Get latest end dates as a target team
                    SELECT 
                        TargetId as TeamId, 
                        MAX([End]) as EndDate
                    FROM [Organization].[TeamMemberships]
                    WHERE IsDeleted = 0
                    GROUP BY TargetId
                ) x
                GROUP BY TeamId
            )
            UPDATE t
            SET InactiveDate = CAST(
                CASE 
                    WHEN m.LatestEnd IS NULL THEN t.LastModified  -- No memberships, use LastModified
                    ELSE m.LatestEnd                              -- Has memberships, use latest end date
                END as date)
            FROM [Organization].[Teams] t
            LEFT JOIN LatestMemberships m ON t.Id = m.TeamId
            WHERE t.IsActive = 0;");


        // Audit Trail for the Team Date Changes
        migrationBuilder.Sql(@"
            DECLARE @CorrelationId uniqueidentifier = NEWID();

            INSERT INTO [Auditing].[AuditTrails] (
                [Id],
                [UserId],
                [Type],
                [TableName],
                [DateTime],
                [OldValues],
                [NewValues],
                [AffectedColumns],
                [PrimaryKey],
                [CorrelationId],
                [SchemaName]
            )
            SELECT 
                NEWID(),                           -- New GUID for each audit record
                '00000000-0000-0000-0000-000000000000',  -- System UserId for migration
                'Update',                          -- Type
                'Team',                           -- TableName
                SYSUTCDATETIME(),                 -- Current UTC time
                JSON_QUERY(                        -- OldValues
                    '{' +
                        '""activeDate"": null' +
                        CASE WHEN t.InactiveDate IS NOT NULL THEN ',""inactiveDate"": null' ELSE '' END +
                    '}'
                ),
                JSON_QUERY(                        -- NewValues
                    '{' +
                        '""activeDate"": ""' + CONVERT(varchar, t.ActiveDate, 23) + '""' +
                        CASE 
                            WHEN t.InactiveDate IS NOT NULL 
                            THEN ',""inactiveDate"": ""' + CONVERT(varchar, t.InactiveDate, 23) + '""'
                            ELSE ''
                        END +
                    '}'
                ),
                JSON_QUERY(                        -- AffectedColumns
                    CASE 
                        WHEN t.InactiveDate IS NOT NULL 
                        THEN '[""ActiveDate"",""InactiveDate""]'
                        ELSE '[""ActiveDate""]'
                    END
                ),
                JSON_QUERY(                        -- PrimaryKey
                    CONCAT('{""id"":""', t.Id, '""}')
                ),
                @CorrelationId,                   -- CorrelationId  
                'Organization'                     -- SchemaName
            FROM [Organization].[Teams] t
            WHERE t.ActiveDate <> CAST('0001-01-01' as date);");


        // MIGRATION FOR GRAPH TABLES
        // Create the TeamNodes table as a graph NODE
        migrationBuilder.Sql(@"
           CREATE TABLE [Organization].[TeamNodes] (
               [Id] UNIQUEIDENTIFIER NOT NULL,
               [Key] INT NOT NULL,
               [Name] varchar(128) NOT NULL,
               [Code] varchar(10) NOT NULL,
               [Type] varchar(32) NOT NULL,
               [IsActive] bit NOT NULL DEFAULT 1,
               [ActiveDate] datetime2(7) NOT NULL,
               [InactiveDate] datetime2(7) NULL,
               CONSTRAINT [PK_TeamNodes] PRIMARY KEY ([Id]),
               CONSTRAINT [AK_TeamNodes_Key] UNIQUE ([Key])
           ) AS NODE;

           CREATE INDEX [IX_TeamNodes_Id] ON [Organization].[TeamNodes] ([Id])
           INCLUDE ([Key], [Name], [Code], [IsActive]);

           CREATE INDEX [IX_TeamNodes_Key] ON [Organization].[TeamNodes] ([Key])
           INCLUDE ([Id], [Name], [Code], [IsActive]);

           CREATE UNIQUE INDEX [IX_TeamNodes_Name] ON [Organization].[TeamNodes] ([Name]);

           CREATE UNIQUE INDEX [IX_TeamNodes_Code] ON [Organization].[TeamNodes] ([Code])
           INCLUDE ([Id], [Key], [Name], [IsActive]);

           CREATE INDEX [IX_TeamNodes_IsActive] ON [Organization].[TeamNodes] ([IsActive]);

           CREATE INDEX [IX_TeamNodes_ActiveDates] ON [Organization].[TeamNodes] 
           ([ActiveDate], [InactiveDate]);");

        // Create the TeamMembershipEdges table as a graph EDGE
        migrationBuilder.Sql(@"
           CREATE TABLE [Organization].[TeamMembershipEdges] (
               [Id] UNIQUEIDENTIFIER NOT NULL,
               [StartDate] datetime2(7) NOT NULL,
               [EndDate] datetime2(7) NULL,
               CONSTRAINT [PK_TeamMembershipEdges] PRIMARY KEY ([Id])
           ) AS EDGE;

           -- Index for finding active memberships as of a date
           CREATE INDEX [IX_TeamMembershipEdges_Active] ON [Organization].[TeamMembershipEdges] 
           (StartDate, EndDate)
           INCLUDE ($from_id, $to_id);

           -- Index for temporal range queries
           CREATE INDEX [IX_TeamMembershipEdges_DateRange] ON [Organization].[TeamMembershipEdges] 
           (EndDate, StartDate)
           INCLUDE ($from_id, $to_id);

           -- Indexes for graph traversal
           CREATE INDEX [IX_TeamMembershipEdges_FromNode] ON [Organization].[TeamMembershipEdges] 
           ($from_id, StartDate, EndDate);

           CREATE INDEX [IX_TeamMembershipEdges_ToNode] ON [Organization].[TeamMembershipEdges] 
           ($to_id, StartDate, EndDate);");

        //Initial data population with proper temporal alignment
        migrationBuilder.Sql(@"
            INSERT INTO [Organization].[TeamNodes] 
                ([Id], [Key], [Name], [Code], [Type], [IsActive], [ActiveDate], [InactiveDate])
            SELECT 
                t.[Id], t.[Key], t.[Name], t.[Code], t.[Type], t.[IsActive],
                t.[ActiveDate],                 -- Use the new ActiveDate field
                t.[InactiveDate]               -- Use the new InactiveDate field
            FROM [Organization].[Teams] t
			WHERE t.[IsDeleted] = 0;

            INSERT INTO [Organization].[TeamMembershipEdges] 
                ([Id], [StartDate], [EndDate], $from_id, $to_id)
            SELECT 
                m.[Id], m.[Start], m.[End], 
                n1.$node_id, n2.$node_id
            FROM [Organization].[TeamMemberships] m
            JOIN [Organization].[TeamNodes] n1 ON m.[SourceId] = n1.[Id]
            JOIN [Organization].[TeamNodes] n2 ON m.[TargetId] = n2.[Id]
			WHERE m.[IsDeleted] = 0;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TeamMembershipEdges",
            schema: "Organization");

        migrationBuilder.DropTable(
            name: "TeamNodes",
            schema: "Organization");

        migrationBuilder.DropColumn(
            name: "ActiveDate",
            schema: "Organization",
            table: "Teams");

        migrationBuilder.DropColumn(
            name: "InactiveDate",
            schema: "Organization",
            table: "Teams");
    }
}
