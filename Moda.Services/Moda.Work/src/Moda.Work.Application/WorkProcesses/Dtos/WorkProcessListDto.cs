﻿using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.WorkProcesses.Dtos;
public sealed record WorkProcessListDto : IMapFrom<WorkProcess>
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required SimpleNavigationDto Ownership { get; set; }
    public bool IsActive { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkProcess, WorkProcessListDto>()
            .Map(dest => dest.Ownership, src => SimpleNavigationDto.FromEnum(src.Ownership));
    }
}
