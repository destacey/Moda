using Mapster;
using Moda.Common.Domain.FeatureManagement;

namespace Moda.Common.Application.FeatureManagement.Dtos;

public sealed record FeatureFlagDto : IMapFrom<FeatureFlag>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsArchived { get; set; }
    public Instant Created { get; set; }
    public Instant LastModified { get; set; }
}
