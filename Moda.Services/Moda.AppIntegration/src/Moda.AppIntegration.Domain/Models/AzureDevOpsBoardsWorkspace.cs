﻿using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkspace
{
    private string _name = null!;
    private string? _description;

    private AzureDevOpsBoardsWorkspace() { }
    private AzureDevOpsBoardsWorkspace(Guid externalId, string name, string? description, Guid? workProcessId, bool import)
    {
        ExternalId = externalId;
        Name = name;
        Description = description;
        WorkProcessId = workProcessId;
        Sync = import;
    }


    public Guid ExternalId { get; init; }

    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets or sets the work process identifier.</summary>
    public Guid? WorkProcessId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this workspace is configured to sync data.
    /// </summary>
    /// <value><c>true</c> if sync; otherwise, <c>false</c>.</value>
    public bool Sync { get; private set; }

    public Result Update(string name, string? description, Guid? workProcessId, bool import)
    {
        try
        {
            Name = name;
            Description = description;
            WorkProcessId = workProcessId;
            Sync = import;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsWorkspace Create(Guid id, string name, string? description, Guid? workProcessId)
    {
        return new AzureDevOpsBoardsWorkspace(id, name, description, workProcessId, false);
    }
}
