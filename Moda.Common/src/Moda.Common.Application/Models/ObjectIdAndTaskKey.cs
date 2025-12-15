namespace Moda.Common.Application.Models;

/// <summary>
/// Represents an object's ID and task key (e.g., "APOLLO-T001").
/// </summary>
public sealed record ObjectIdAndTaskKey(Guid Id, string TaskKey);
