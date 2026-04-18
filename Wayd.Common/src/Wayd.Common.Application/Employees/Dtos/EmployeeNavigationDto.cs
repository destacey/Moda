using Mapster;
using Wayd.Common.Application.Dtos;
using Wayd.Common.Domain.Employees;

namespace Wayd.Common.Application.Employees.Dtos;

public sealed record EmployeeNavigationDto : NavigationDto, IMapFrom<Employee>
{
    public static EmployeeNavigationDto From(Employee employee)
        => new()
        {
            Id = employee.Id,
            Key = employee.Key,
            Name = employee.Name.DisplayName
        };

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Employee, EmployeeNavigationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Key, src => src.Key)
            .Map(dest => dest.Name, src => src.Name.FirstName + " " + src.Name.LastName);
    }
}
