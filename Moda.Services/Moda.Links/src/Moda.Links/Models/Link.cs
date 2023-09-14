using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;

namespace Moda.Links.Models;

public sealed class Link : BaseEntity<Guid>
{
    private string _name = default!;
    private string _url = default!;

    private Link() { }
    private Link(Guid objectId, string name, string url)
    {
        ObjectId = objectId;
        Name = name;
        Url = url;
    }

    public Guid ObjectId { get; set; }
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }
    public string Url
    {
        get => _url;
        private set => _url = Guard.Against.NullOrWhiteSpace(value, nameof(Url)).Trim();
    }

    // update
    public Result Update(string name, string url)
    {
        Name = name;
        Url = url;

        return Result.Success();
    }

    public static Link Create(Guid objectId, string name, string url)
    {
        return new Link(objectId, name, url);
    }
}
