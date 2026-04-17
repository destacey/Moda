using Mapster;
using Wayd.Common.Domain.Identity;

namespace Wayd.Common.Application.Identity.Users;

public sealed record UserNavigationDto : IMapFrom<User>
{
    public required Guid Id { get; set; }
    public required string UserName { get; set; }
    public string? Name { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserNavigationDto>()
            .Map(dest => dest.Name, src => src.DisplayName);
    }
}
