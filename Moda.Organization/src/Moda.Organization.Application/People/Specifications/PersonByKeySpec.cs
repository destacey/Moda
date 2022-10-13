using Ardalis.Specification;

namespace Moda.Organization.Application.People.Specifications;
public sealed class PersonByKeySpec : Specification<Person>, ISingleResultSpecification
{
    public PersonByKeySpec(string key)
    {
        Query.Where(x => x.Key == key);
    }
}
