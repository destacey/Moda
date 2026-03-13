using Moda.Common.Application.Persistence;
using Moda.Common.Domain.FeatureManagement;

namespace Moda.Common.Application.FeatureManagement;

public interface IFeatureManagementDbContext : IModaDbContext
{
    DbSet<FeatureFlag> FeatureFlags { get; }
}
