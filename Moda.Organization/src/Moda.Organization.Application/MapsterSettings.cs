using Mapster;

namespace Moda.Organization.Application;
public class MapsterSettings
{
    public static void Configure()
    {
        // here we will define the type conversion / Custom-mapping
        // More details at https://github.com/MapsterMapper/Mapster/wiki/Custom-mapping

        TypeAdapterConfig<Employee, EmployeeListDto>.NewConfig()
            .Map(dest => dest.FirstName, src => src.Name.FirstName)
            .Map(dest => dest.MiddleName, src => src.Name.MiddleName)
            .Map(dest => dest.LastName, src => src.Name.LastName)
            .Map(dest => dest.Suffix, src => src.Name.Suffix)
            .Map(dest => dest.Title, src => src.Name.Title)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.ManagerLocalId, src => src.Manager!.LocalId)
            .Map(dest => dest.ManagerName, src => $"{src.Manager!.Name.FirstName} {src.Manager!.Name.LastName}", srcCond => srcCond.ManagerId.HasValue);

        TypeAdapterConfig<Employee, EmployeeDetailsDto>.NewConfig()
            .Map(dest => dest.FirstName, src => src.Name.FirstName)
            .Map(dest => dest.MiddleName, src => src.Name.MiddleName)
            .Map(dest => dest.LastName, src => src.Name.LastName)
            .Map(dest => dest.Suffix, src => src.Name.Suffix)
            .Map(dest => dest.Title, src => src.Name.Title)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.ManagerLocalId, src => src.Manager!.LocalId)
            .Map(dest => dest.ManagerName, src => $"{src.Manager!.Name.FirstName} {src.Manager!.Name.LastName}", srcCond => srcCond.ManagerId.HasValue);
    }
}
