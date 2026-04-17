using Wayd.Common.Application.Requests.WorkManagement.Interfaces;

namespace Wayd.Common.Application.Requests.WorkManagement.Queries;

public sealed record GetWorkProcessSchemesQuery(Guid WorkProcessId) : IQuery<IReadOnlyList<IWorkProcessSchemeDto>>;

public sealed class GetWorkProcessSchemesQueryValidator : CustomValidator<GetWorkProcessSchemesQuery>
{
    public GetWorkProcessSchemesQueryValidator()
    {
        RuleFor(q => q.WorkProcessId)
            .NotEmpty();
    }
}
