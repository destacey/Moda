namespace Moda.Common.Domain.Interfaces;

public interface ISoftDelete
{
    bool IsDeleted { get; }

    //bool CanDelete();
}
