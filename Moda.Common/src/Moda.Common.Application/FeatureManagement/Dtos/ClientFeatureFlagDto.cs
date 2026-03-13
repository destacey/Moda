using Mapster;
using Moda.Common.Domain.FeatureManagement;

namespace Moda.Common.Application.FeatureManagement.Dtos;

public sealed record ClientFeatureFlagDto : IMapFrom<FeatureFlag>
{
    public string Name { get; set; } = default!;
    public bool IsEnabled { get; set; }
}
