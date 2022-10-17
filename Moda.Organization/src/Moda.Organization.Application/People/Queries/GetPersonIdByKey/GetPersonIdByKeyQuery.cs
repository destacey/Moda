using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;

namespace Moda.Organization.Application.People.Queries.GetPersonByKey;

public sealed record GetPersonIdByKeyQuery : IRequest<Guid?>
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

public sealed class GetPersonIdByKeyQueryHandler : IRequestHandler<GetPersonIdByKeyQuery, Guid?>
{
    private readonly IReadRepository<Person> _readRepository;

    public GetPersonIdByKeyQueryHandler(IReadRepository<Person> readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Guid?> Handle(GetPersonIdByKeyQuery request, CancellationToken cancellationToken)
    {
        var personId = await _readRepository.SingleOrDefaultAsync(new PersonIdByKeySpec(request.Key), cancellationToken);

        return personId;
    }
}
