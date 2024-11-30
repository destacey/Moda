using NodaTime;

namespace Moda.Organization.Application.Teams.Dtos;
public sealed record FunctionalOrganizationChartDto
{
    public required LocalDate AsOfDate { get; init; }
    public List<OrganizationalUnitDto> Organization { get; init; } = [];
    public int Total { get; init; }
    public int MaxDepth { get; init; }
}
