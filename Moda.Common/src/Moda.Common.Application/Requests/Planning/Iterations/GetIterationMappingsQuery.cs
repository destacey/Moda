using Moda.Common.Domain.Enums.AppIntegrations;

namespace Moda.Common.Application.Requests.Planning.Iterations;
public sealed record GetIterationMappingsQuery(Connector Connector, string SystemId) : IQuery<Dictionary<string, Guid>>;

public sealed class GetIterationMappingsQueryValidator : CustomValidator<GetIterationMappingsQuery>
{
    public GetIterationMappingsQueryValidator()
    {
        RuleFor(q => q.Connector)
            .IsInEnum();

        RuleFor(q => q.SystemId)
            .NotEmpty();
    }
}
