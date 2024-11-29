namespace Moda.Organization.Application.Teams.Dtos;
public sealed record FunctionalOrganizationChartDto
{
    public List<OrganizationalUnitDto> Organization { get; set; } = [];
}
