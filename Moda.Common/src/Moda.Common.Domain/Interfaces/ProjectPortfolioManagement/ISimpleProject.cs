using Moda.Common.Domain.Models.ProjectPortfolioManagement;

namespace Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;

public interface ISimpleProject
{
    Guid Id { get; }
    ProjectKey Key { get; }
    string Name { get; }
    string Description { get; }
}
