using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moda.Organization.Application.Persistence;

namespace Moda.Organization.Application.People.Queries.PersonExistsByKey;

public sealed record PersonExistsByKeyQuery : IQuery<bool>
{
    public PersonExistsByKeyQuery(string key)
    {
        Key = key;
    }
    public string Key { get; } = default!;
}

public sealed class PersonExistsByKeyQueryValidator : CustomValidator<PersonExistsByKeyQuery>
{
    public PersonExistsByKeyQueryValidator()
    {
        RuleFor(q => q.Key)
            .NotEmpty();
    }
}

internal sealed class PersonExistsByKeyQueryHandler : IQueryHandler<PersonExistsByKeyQuery, bool>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public PersonExistsByKeyQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<Result<bool>> Handle(PersonExistsByKeyQuery request, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.People.AnyAsync(p => p.Key == request.Key, cancellationToken);
    }
}