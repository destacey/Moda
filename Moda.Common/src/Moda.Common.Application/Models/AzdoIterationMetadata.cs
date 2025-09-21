namespace Moda.Common.Application.Models;
public sealed class AzdoIterationMetadata
{
    public Guid Identifier { get; set; }
    public required string Path { get; set; } = null!;
    public bool HasChildren { get; set; }
}
