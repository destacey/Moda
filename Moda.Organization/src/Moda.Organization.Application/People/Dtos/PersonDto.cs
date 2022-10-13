namespace Moda.Organization.Application.People.Dtos;
public sealed record PersonDto : IDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
}
