﻿using Moda.Common.Domain.Enums;

namespace Moda.Common.Application.Requests;

public sealed record GetVisibilitiesQuery : IQuery<List<VisibilityDto>> { }

internal sealed class GetVisibilitiesQueryHandler : IQueryHandler<GetVisibilitiesQuery, List<VisibilityDto>>
{
    public Task<List<VisibilityDto>> Handle(GetVisibilitiesQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CommonEnumDto.GetValues<Visibility, VisibilityDto>());
    }
}

public sealed record VisibilityDto : CommonEnumDto { }