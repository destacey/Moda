using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface IActivatable
{
    bool IsActive { get; }
    Result Activate(Instant activatedOn);
    Result Deactivate(Instant deactivatedOn);
}
