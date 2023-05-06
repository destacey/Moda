using Mapster;
using Moda.Common.Application.Dtos;

namespace Moda.Infrastructure.Identity;
public class IdentityMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<ApplicationUser, UserDetailsDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.Employee, src => src.Employee == null
                    ? null
                    : new NavigationDto
                    {
                        Id = src.Employee.Id,
                        LocalId = src.Employee.LocalId,
                        Name = src.Employee.Name.FullName,
                    });
    }
}
