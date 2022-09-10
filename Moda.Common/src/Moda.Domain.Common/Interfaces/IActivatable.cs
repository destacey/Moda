namespace Moda.Common.Domain.Interfaces;

public interface IActivatable
{
    bool IsActive { get; }
    void Activate();
    void Deactivate();
}
