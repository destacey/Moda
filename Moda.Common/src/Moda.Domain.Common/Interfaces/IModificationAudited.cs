namespace Moda.Common.Domain.Interfaces;

public interface IModificationAudited : IHasModificationDateTime
{
    string? LastModifiedBy { get; }
}
