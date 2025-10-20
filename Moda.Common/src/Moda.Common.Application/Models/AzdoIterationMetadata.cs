namespace Moda.Common.Application.Models;
public sealed class AzdoIterationMetadata
{
    public Guid ProjectId { get; set; }
    public Guid Identifier { get; set; }
    public required string Path { get; set; }
}
