using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Moda.Infrastructure.Persistence.Configuration;
public class EnumConverter<T> : ValueConverter<T, string>
{
    public EnumConverter()
        : base(
            e => e!.ToString()!,
            e => (T)Enum.Parse(typeof(T), e))
    {
    }
}
