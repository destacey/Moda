

namespace Moda.AppIntegration.Application.Connectors.Queries;

public sealed record GetConnectorsQuery : IQuery<IReadOnlyList<ConnectorListDto>> { }

internal sealed class GetConnectorsQueryHandler : IQueryHandler<GetConnectorsQuery, IReadOnlyList<ConnectorListDto>>
{
    public Task<IReadOnlyList<ConnectorListDto>> Handle(GetConnectorsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ConnectorListDto> values = Enum.GetValues<Connector>().Select(c => new ConnectorListDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription()
        }).ToList();

        return Task.FromResult(values);
    }
}
