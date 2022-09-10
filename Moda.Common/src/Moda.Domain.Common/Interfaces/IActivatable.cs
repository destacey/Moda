using CSharpFunctionalExtensions;

namespace Moda.Common.Domain.Interfaces;

public interface IActivatable
{
    bool IsActive { get; }
    Result Activate();
    Result Deactivate();
}
