using OneOf;

namespace Moda.Common.Application.Models;

public class IdOrKey : OneOfBase<Guid, int>
{
    public IdOrKey(OneOf<Guid, int> value) : base(value) { }

    public IdOrKey(string value) : base(Guid.TryParse(value, out var guid) ? guid : int.Parse(value)) { }

    public static implicit operator IdOrKey(Guid value) => new(value);
    public static implicit operator IdOrKey(int value) => new(value);
    public static implicit operator IdOrKey(string value) => new(value);
}
