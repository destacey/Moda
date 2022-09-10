namespace Moda.Common.Domain.Interfaces;

public interface ICreationAudited : IHasCreationDateTime
{
    string? CreatedBy { get; }
}
