using Wayd.Common.Application.Persistence;
using Wayd.Common.Domain.FeatureManagement;

namespace Wayd.Common.Application.FeatureManagement;

public interface IFeatureManagementDbContext : IModaDbContext
{
    DbSet<FeatureFlag> FeatureFlags { get; }
}
