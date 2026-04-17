using Wayd.Common.Domain.Models.ProjectPortfolioManagement;

namespace Wayd.Common.Domain.Interfaces.ProjectPortfolioManagement;

public interface ISimpleProject
{
    Guid Id { get; }
    ProjectKey Key { get; }
    string Name { get; }
    string Description { get; }
}
