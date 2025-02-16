﻿using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
public sealed record ProjectPortfolioListDto : IMapFrom<ProjectPortfolio>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the portfolio.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the portfolio.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the portfolio.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The status of the portfolio.
    /// </summary>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>
    /// The sponsors of the portfolio.
    /// </summary>
    public required List<EmployeeNavigationDto> PortfolioSponsors { get; set; } = [];

    /// <summary>
    /// The owners of the portfolio.
    /// </summary>
    public required List<EmployeeNavigationDto> PortfolioOwners { get; set; } = [];

    /// <summary>
    /// The managers of the portfolio.
    /// </summary>
    public required List<EmployeeNavigationDto> PortfolioManagers { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectPortfolio, ProjectPortfolioListDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.PortfolioSponsors, src => src.Roles.Where(r => r.Role == ProjectPortfolioRole.Sponsor).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.PortfolioOwners, src => src.Roles.Where(r => r.Role == ProjectPortfolioRole.Owner).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList())
            .Map(dest => dest.PortfolioManagers, src => src.Roles.Where(r => r.Role == ProjectPortfolioRole.Manager).Select(x => EmployeeNavigationDto.From(x.Employee!)).ToList());
    }
}
