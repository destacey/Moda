namespace Moda.AppIntegration.Application.Connectors.Queries;
public sealed record GetConnectorTypesQuery : IQuery<IReadOnlyList<ConnectorTypeDto>> { }

internal sealed class GetConnectorTypesQueryHandler : IQueryHandler<GetConnectorTypesQuery, IReadOnlyList<ConnectorTypeDto>>
{
    public Task<IReadOnlyList<ConnectorTypeDto>> Handle(GetConnectorTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ConnectorTypeDto> values = Enum.GetValues<ConnectorType>().Select(ct => new ConnectorTypeDto
        {
            Id = (int)ct,
            Name = ct.GetDisplayName()
        }).ToList();

        return Task.FromResult(values);
    }
}
