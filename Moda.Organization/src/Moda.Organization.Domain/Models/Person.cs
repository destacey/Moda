using Ardalis.GuardClauses;
using NodaTime;

namespace Moda.Organization.Domain.Models;
public class Person : BaseEntity<Guid>
{
    private Person() { }

    private Person(Guid id, string key)
    {
        Id = Guard.Against.Default(id);
        Key = Guard.Against.NullOrWhiteSpace(key).Trim();
    }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public string Key { get; private set; } = null!;

    /// <summary>Creates a Person using a key and adds a domain event with the timestamp.</summary>
    /// <param name="key">The key.</param>
    /// <param name="timestamp">The timestamp for the domain event.</param>
    /// <returns>A Person</returns>
    public static Person Create(string key, Instant timestamp)
    {
        Person person = new (Guid.NewGuid(), key);
        person.AddDomainEvent(EntityCreatedEvent.WithEntity(person, timestamp));
        return person;
    }

    /// <summary>Creates a Person using the id and key from an existing entity.</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="key">The key.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns>A Person</returns>
    public static Person CreateFromExisting(Guid id, string key, Instant timestamp)
    {
        Person person = new(id, key);
        person.AddDomainEvent(EntityCreatedEvent.WithEntity(person, timestamp));
        return person;
    }
}
