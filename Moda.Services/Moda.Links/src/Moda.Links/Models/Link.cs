using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;

namespace Moda.Links.Models;

public sealed class Link : BaseEntity
{
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
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = default!;
    public string Url
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Url)).Trim();
    } = default!;

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
