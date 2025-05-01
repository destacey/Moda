namespace Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;

public interface ISimpleProject
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    string Description { get; }
}
