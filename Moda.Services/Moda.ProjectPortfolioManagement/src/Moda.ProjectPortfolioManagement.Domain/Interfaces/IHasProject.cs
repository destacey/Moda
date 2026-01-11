using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Domain.Interfaces;

public interface IHasProject
{
    Guid ProjectId { get; }
    Project Project { get; }
}
