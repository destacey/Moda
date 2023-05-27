﻿using Moda.Common.Application.Dtos;
using Moda.Common.Extensions;
using Moda.Planning.Application.Models;

namespace Moda.Planning.Application.Risks.Dtos;
public class RiskDetailsDto : IMapFrom<Risk>
{
    public Guid Id { get; set; }
    public int LocalId { get; set; }
    public string Summary { get; set; } = default!;
    public string? Description { get; set; }
    public PlanningTeamNavigationDto? Team { get; set; } = default!;
    public Instant ReportedOn { get; set; }
    public NavigationDto ReportedBy { get; set; } = default!;
    public SimpleNavigationDto Status { get; set; } = default!;
    public SimpleNavigationDto Category { get; set; } = default!;
    public SimpleNavigationDto Impact { get; set; } = default!;
    public SimpleNavigationDto Likelihood { get; set; } = default!;
    public SimpleNavigationDto Exposure { get; set; } = default!;
    public NavigationDto? Assignee { get; set; }
    public LocalDate? FollowUpDate { get; set; }
    public string? Response { get; set; }
    public Instant? ClosedDate { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Risk, RiskDetailsDto>()
            .Map(dest => dest.ReportedBy, src => NavigationDto.Create(src.ReportedBy.Id, src.ReportedBy.LocalId, src.ReportedBy.Name.FullName))
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Category, src => SimpleNavigationDto.FromEnum(src.Category))
            .Map(dest => dest.Impact, src => SimpleNavigationDto.FromEnum(src.Impact))
            .Map(dest => dest.Likelihood, src => SimpleNavigationDto.FromEnum(src.Likelihood))
            .Map(dest => dest.Exposure, src => SimpleNavigationDto.FromEnum(src.Exposure))
            .Map(dest => dest.Assignee, src => src.Assignee == null ? null : NavigationDto.Create(src.Assignee.Id, src.Assignee.LocalId, src.Assignee.Name.FullName));
    }
}
