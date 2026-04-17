using Wayd.ProjectPortfolioManagement.Domain.Models;

namespace Wayd.ProjectPortfolioManagement.Domain.Interfaces;

public interface IHasProject
{
    Guid ProjectId { get; }
    Project Project { get; }
}
