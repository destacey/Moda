using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.People.Queries;

public sealed record GetPersonIdByKeyQuery : IQuery<Guid?>
{
    public GetPersonIdByKeyQuery(string key)
    {
        Key = key;
    }
    public string Key { get; } = default!;
}

public sealed class GetPersonIdByKeyQueryValidator : CustomValidator<GetPersonIdByKeyQuery>
{
    public GetPersonIdByKeyQueryValidator()
    {
        RuleFor(q => q.Key)
            .NotEmpty();
    }
}

internal sealed class GetPersonIdByKeyQueryHandler : IQueryHandler<GetPersonIdByKeyQuery, Guid?>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public GetPersonIdByKeyQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<Guid?> Handle(GetPersonIdByKeyQuery request, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.People
            .Where(p => p.Key == request.Key)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
