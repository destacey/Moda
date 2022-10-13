namespace Moda.Organization.Domain.Models;
public class Person : BaseEntity<Guid>, IAggregateRoot
{
    private Person() { }

    public Person(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(Constants.IsNullOrWhiteSpaceExceptionMessage, nameof(key));

        Key = key;
    }

    public string Key { get; } = default!;
}
