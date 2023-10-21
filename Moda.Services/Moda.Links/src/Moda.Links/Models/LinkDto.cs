using Mapster;
using Moda.Common.Application.Interfaces;

namespace Moda.Links.Models;
public sealed record LinkDto : IMapFrom<Link>
{
    public Guid Id { get; set; }
    public Guid ObjectId { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Link, LinkDto>();
    }
}
