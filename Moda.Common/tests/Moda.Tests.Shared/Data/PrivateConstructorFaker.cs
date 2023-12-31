using Bogus;
using Moda.Tests.Shared.Extensions;

namespace Moda.Tests.Shared.Data;
public class PrivateConstructorFaker<T> : Faker<T> where T : class
{
    public PrivateConstructorFaker() : base("en", IncludePrivateFieldBinder.Create())
    {
        CreateActions[Default] = fakerOfT =>
            this.UsePrivateConstructor() as PrivateConstructorFaker<T>;
    }
}
