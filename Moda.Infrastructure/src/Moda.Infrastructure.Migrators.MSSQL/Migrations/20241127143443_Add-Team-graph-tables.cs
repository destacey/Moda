using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moda.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class AddTeamgraphtables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create the TeamNodes table as a graph NODE
        migrationBuilder.Sql(@"
           CREATE TABLE [Organization].[TeamNodes] (
               [Id] UNIQUEIDENTIFIER NOT NULL,
               [Key] INT NOT NULL,
               [Name] varchar(128) NOT NULL,
               [Code] varchar(10) NOT NULL,
               [Type] varchar(32) NOT NULL,
               [IsActive] bit NOT NULL DEFAULT 1,
               [IsDeleted] bit NOT NULL DEFAULT 0,
               [ActiveTimestamp] datetime2(7) NOT NULL,
               [InactiveTimestamp] datetime2(7) NULL,
               CONSTRAINT [PK_TeamNodes] PRIMARY KEY ([Id]),
               CONSTRAINT [AK_TeamNodes_Key] UNIQUE ([Key])
           ) AS NODE;

           CREATE INDEX [IX_TeamNodes_Id_IsDeleted] ON [Organization].[TeamNodes] ([Id], [IsDeleted])
           INCLUDE ([Key], [Name], [Code], [IsActive]) 
           WHERE [IsDeleted] = 0;

           CREATE INDEX [IX_TeamNodes_Key_IsDeleted] ON [Organization].[TeamNodes] ([Key], [IsDeleted])
           INCLUDE ([Id], [Name], [Code], [IsActive]) 
           WHERE [IsDeleted] = 0;

           CREATE UNIQUE INDEX [IX_TeamNodes_Name] ON [Organization].[TeamNodes] ([Name])
           WHERE [IsDeleted] = 0;

           CREATE UNIQUE INDEX [IX_TeamNodes_Code] ON [Organization].[TeamNodes] ([Code])
           INCLUDE ([Id], [Key], [Name], [IsActive], [IsDeleted])
           WHERE [IsDeleted] = 0;

           CREATE INDEX [IX_TeamNodes_IsActive_IsDeleted] ON [Organization].[TeamNodes] ([IsActive], [IsDeleted])
           WHERE [IsDeleted] = 0;

           CREATE INDEX [IX_TeamNodes_ActiveTimestamps] ON [Organization].[TeamNodes] 
           ([ActiveTimestamp], [InactiveTimestamp], [IsDeleted])
           WHERE [IsDeleted] = 0;");

        // Create the TeamMembershipEdges table as a graph EDGE
        migrationBuilder.Sql(@"
           CREATE TABLE [Organization].[TeamMembershipEdges] (
               [Id] UNIQUEIDENTIFIER NOT NULL,
               [StartDate] datetime2(7) NOT NULL,
               [EndDate] datetime2(7) NULL,
               [IsDeleted] bit NOT NULL DEFAULT 0,
               CONSTRAINT [PK_TeamMembershipEdges] PRIMARY KEY ([Id])
           ) AS EDGE;

           -- Index for finding active memberships as of a date
           CREATE INDEX [IX_TeamMembershipEdges_Active] ON [Organization].[TeamMembershipEdges] 
           (StartDate, EndDate, IsDeleted)
           INCLUDE ($from_id, $to_id)
           WHERE IsDeleted = 0;

           -- Index for temporal range queries
           CREATE INDEX [IX_TeamMembershipEdges_DateRange] ON [Organization].[TeamMembershipEdges] 
           (EndDate, StartDate, IsDeleted)
           INCLUDE ($from_id, $to_id)
           WHERE IsDeleted = 0;

           -- Indexes for graph traversal
           CREATE INDEX [IX_TeamMembershipEdges_FromNode] ON [Organization].[TeamMembershipEdges] 
           ($from_id, StartDate, EndDate, IsDeleted)
           WHERE IsDeleted = 0;

           CREATE INDEX [IX_TeamMembershipEdges_ToNode] ON [Organization].[TeamMembershipEdges] 
           ($to_id, StartDate, EndDate, IsDeleted)
           WHERE IsDeleted = 0;");

        //Initial data population with proper temporal alignment
        migrationBuilder.Sql(@"
            -- First, insert all teams with their basic ActiveTimestamp
            INSERT INTO [Organization].[TeamNodes] 
                ([Id], [Key], [Name], [Code], [Type], [IsActive], [IsDeleted], [ActiveTimestamp], [InactiveTimestamp])
            SELECT 
                t.[Id], t.[Key], t.[Name], t.[Code], t.[Type], t.[IsActive], t.[IsDeleted],
                t.[Created] as [ActiveTimestamp],
                CASE WHEN t.[IsActive] = 0 THEN t.[Deleted] ELSE NULL END as [InactiveTimestamp]
            FROM [Organization].[Teams] t;

            -- Create a temp table to store earliest dates for each team based on memberships
            WITH MembershipDates AS (
                -- Get earliest dates as a source team
                SELECT 
                    m.SourceId as TeamId,
                    MIN(m.[Start]) as EarliestMembershipDate
                FROM [Organization].[TeamMemberships] m
                WHERE m.IsDeleted = 0
                GROUP BY m.SourceId
        
                UNION
        
                -- Get earliest dates as a target team
                SELECT 
                    m.TargetId as TeamId,
                    MIN(m.[Start]) as EarliestMembershipDate
                FROM [Organization].[TeamMemberships] m
                WHERE m.IsDeleted = 0
                GROUP BY m.TargetId
            ),
            ConsolidatedDates AS (
                SELECT 
                    TeamId,
                    MIN(EarliestMembershipDate) as EarliestDate
                FROM MembershipDates
                GROUP BY TeamId
            )
            -- Update the ActiveTimestamp where membership dates are earlier
            UPDATE n
            SET n.ActiveTimestamp = CASE 
                    WHEN d.EarliestDate < n.ActiveTimestamp THEN d.EarliestDate
                    ELSE n.ActiveTimestamp
                END
            FROM [Organization].[TeamNodes] n
            JOIN ConsolidatedDates d ON n.Id = d.TeamId;

            -- Insert memberships after nodes are properly aligned
            INSERT INTO [Organization].[TeamMembershipEdges] 
                ([Id], [StartDate], [EndDate], [IsDeleted], $from_id, $to_id)
            SELECT 
                m.[Id], m.[Start], m.[End], m.[IsDeleted], 
                n1.$node_id, n2.$node_id
            FROM [Organization].[TeamMemberships] m
            JOIN [Organization].[TeamNodes] n1 ON m.[SourceId] = n1.[Id]
            JOIN [Organization].[TeamNodes] n2 ON m.[TargetId] = n2.[Id];");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[Organization].[TeamMembershipEdges]'))
                DROP TABLE [Organization].[TeamMembershipEdges];
           
            IF EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[Organization].[TeamNodes]'))
                DROP TABLE [Organization].[TeamNodes];");
    }
}
