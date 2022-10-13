using Ardalis.Specification;

namespace Moda.Organization.Application.People.Specifications;
public sealed class PersonIdByKeySpec : Specification<Person,Guid?>, ISingleResultSpecification<Person,Guid?>
{
    public PersonIdByKeySpec(string key)
    {
        Query.Where(x => x.Key == key);
        Query.Select(p => p.Id);
    }
}
