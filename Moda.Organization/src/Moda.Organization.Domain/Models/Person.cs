using Ardalis.GuardClauses;
using NodaTime;

namespace Moda.Organization.Domain.Models;
public class Person : BaseEntity<Guid>, IAggregateRoot
{
    private Person() { }

    internal Person(string key)
    {
        Key = Guard.Against.NullOrWhiteSpace(key, nameof(key)).Trim();
    }

    public string Key { get; private set; } = default!;

    public static Person Create(string key, Instant timestamp)
    {
        var person = new Person(key);
        person.AddDomainEvent(EntityCreatedEvent.WithEntity(person, timestamp));
        return person;
    }
}
