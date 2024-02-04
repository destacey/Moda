using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// 
/// </summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseAuditableEntity&lt;System.Guid&gt;" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable" />
public sealed class Workflow : BaseAuditableEntity<Guid>, IActivatable
{
    private readonly List<WorkflowScheme> _schemes = [];
    private string _name = null!;
    private string? _description;

    private Workflow() { }

    internal Workflow(string name, string? description, Ownership ownership, Guid? externalId)
    {
        Name = name;
        Description = description;
        Ownership = ownership;
        ExternalId = externalId;
    }

    /// <summary>
    /// The name of the workflow.
    /// </summary>
    public string Name
    {
        get => _name;
        private init => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the workflow.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// Indicates whether the workflow is owned by Moda or a third party system.  This value should not change.
    /// </summary>
    public Ownership Ownership { get; }

    /// <summary>
    /// Gets the external identifier. The value is required when Ownership is managed; otherwise it's null.  For Azure DevOps, this is the process id.
    /// </summary>
    public Guid? ExternalId { get; }

    /// <summary>
    /// Indicates whether the workflow is active or not.  Only active workflows can be assigned 
    /// to work process configurations.  The default is false and the user should activate it after the configuration is complete.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<WorkflowScheme> Schemes => _schemes.AsReadOnly();

    /// <summary>
    /// The process for activating a workflow.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a workflow.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            // TODO is there logic that would prevent deactivation?
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    // TODO move CRUD operations for WorkflowScheme here
    //public Result AddScheme(int workStatusId, WorkStatusCategory workStatusCategory, int order)
    //{
    //    try
    //    {

    //        _schemes.Add(WorkflowScheme.Create(Id, workStatusId, workStatusCategory, order));
    //        return Result.Success();

    //    }
    //    catch (Exception ex)
    //    {
    //        return Result.Failure(ex.ToString());
    //    }
    //}

    public static Workflow Create(string name, string? description, Ownership ownership, Guid? externalId)
    {
        return new(name, description, ownership, externalId);
    }
}
