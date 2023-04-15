using Mapster;

namespace Moda.Organization.Application.Teams.Dtos;
public record NavigationDto : IMapFrom<BaseTeam>
{
    public Guid Id { get; set; }
    public int LocalId { get; set; }
    public required string Name { get; set; }
}
