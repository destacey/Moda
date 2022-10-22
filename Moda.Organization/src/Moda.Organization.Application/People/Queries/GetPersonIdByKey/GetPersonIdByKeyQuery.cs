using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moda.Organization.Application.Persistence;

namespace Moda.Organization.Application.People.Queries.GetPersonByKey;

public sealed record GetPersonIdByKeyQuery : IQuery<Guid>
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

internal sealed class GetPersonIdByKeyQueryHandler : IQueryHandler<GetPersonIdByKeyQuery, Guid>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public GetPersonIdByKeyQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<Result<Guid>> Handle(GetPersonIdByKeyQuery request, CancellationToken cancellationToken)
    {
        Guid? personId = await _organizationDbContext.People
            .Where(p => p.Key == request.Key)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return personId is null 
            ? Result.Failure<Guid>($"Unable to find a Person with Key {request.Key}") 
            : Result.Success(personId.Value);
    }
}
