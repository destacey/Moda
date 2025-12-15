using Moda.Common.Domain.Models.ProjectPortfolioManagement;

namespace Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;

public interface IHasProjectIdAndKey
{
    Guid Id { get; }
    ProjectKey Key { get; }
}
