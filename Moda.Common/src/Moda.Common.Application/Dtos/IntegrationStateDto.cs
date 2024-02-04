using Mapster;
using Moda.Common.Domain.Models;

namespace Moda.Common.Application.Dtos;
public sealed class IntegrationStateDto : IMapFrom<IntegrationState<Guid>>
{
    public required Guid InternalId { get; set; }
    public bool IsActive { get; set; }

    // TODO: not working with generic type
    //public void ConfigureMapping(TypeAdapterConfig config)
    //{
    //    config.NewConfig<IntegrationState<TId>, IntegrationStateDto<TId>>()
    //        .Map(dest => dest.InternalId, src => (TId)src.InternalId)
    //        .Map(dest => dest.IsActive, src => src.IsActive);
    //}
}
