using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface IActivatable
{
    bool IsActive { get; }
    Result Activate(Instant timestamp, Dictionary<string,object>? args = null);
    Result Deactivate(Instant timestamp);
}
