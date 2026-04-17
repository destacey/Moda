using Wayd.Common.Application.Employees.Dtos;
using Wayd.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Wayd.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;

public sealed record StrategicInitiativeKpiMeasurementDto : IMapFrom<StrategicInitiativeKpiMeasurement>
{
    public Guid Id { get; set; }
    public double ActualValue { get; set; }
    public Instant MeasurementDate { get; set; }
    public required EmployeeNavigationDto MeasuredBy { get; set; }
    public string? Note { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<StrategicInitiativeKpiMeasurement, StrategicInitiativeKpiMeasurementDto>()
            .Map(dest => dest.MeasuredBy, src => EmployeeNavigationDto.From(src.MeasuredBy));
    }
}

