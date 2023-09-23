using Bogus;

namespace Moda.Tests.Shared.Extensions;
public static class FakerExtensions
{
    public static Faker<T> UsePrivateConstructor<T>(this Faker<T> faker) where T : class
    {
        return faker.CustomInstantiator(f => 
            Activator.CreateInstance(typeof(T), nonPublic: true) as T 
            ?? throw new ArgumentNullException($"typeof({typeof(T)}) does not have a private constructor."));
    }

    public static Faker<T> RuleForPrivate<T,TProperty>(this Faker<T> faker, string propertyName, Func<Faker, TProperty> setter) where T : class
    {
        return faker.RuleFor(propertyName, setter);
    }
}
