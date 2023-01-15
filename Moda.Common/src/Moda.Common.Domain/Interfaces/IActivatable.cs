using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface IActivatable<TActivate, TDeactivate>
{
    bool IsActive { get; }
    Result Activate(TActivate args);
    Result Deactivate(TDeactivate args);
}

public interface IActivatable<T> : IActivatable<T, T>
{
}

public interface IActivatable : IActivatable<Instant, Instant>
{
}