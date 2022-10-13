using CSharpFunctionalExtensions;
using FluentValidation;

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

public sealed class PersonExistsByKeyQueryHandler : IQueryHandler<PersonExistsByKeyQuery, bool>
{
    private readonly IReadRepository<Person> _readRepository;

    public PersonExistsByKeyQueryHandler(IReadRepository<Person> readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<bool>> Handle(PersonExistsByKeyQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.AnyAsync(new PersonByKeySpec(request.Key), cancellationToken);
    }
}