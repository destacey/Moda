using CSharpFunctionalExtensions;

namespace Moda.Organization.Application.People.Queries.GetPersonByKey;

public sealed record GetPersonByKeyQuery : IQuery<Guid?>
{
    public GetPersonByKeyQuery(string key)
    {
        Key = key;
    }
    public string Key { get; } = default!;
}

public sealed class GetPersonByKeyQueryHandler : IQueryHandler<GetPersonByKeyQuery, Guid?>
{

    public GetPersonByKeyQueryHandler()
    {

    }

    public Task<Result<Guid?>> Handle(GetPersonByKeyQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
