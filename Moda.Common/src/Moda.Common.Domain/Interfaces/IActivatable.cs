using CSharpFunctionalExtensions;

namespace Moda.Common.Domain.Interfaces;

public interface IActivatable<T>
{
    bool IsActive { get; }
    Result Activate(T args);
    Result Deactivate(T args);
}
