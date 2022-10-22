using NodaTime;

namespace Moda.Organization.Domain.Models;
public class Person : BaseEntity<Guid>, IAggregateRoot
{
    private Person() { }

    public Person(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(Constants.IsNullOrWhiteSpaceExceptionMessage, nameof(key));

        Key = key;
    }

    public string Key { get; } = default!;

    public static Person Create(string key, Instant timestamp)
    {
        var person = new Person(key);
        person.AddDomainEvent(EntityCreatedEvent.WithEntity(person, timestamp));
        return person;
    }
}
