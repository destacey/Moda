namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalBacklogLevel
{
    string Id { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
    int Rank { get; set; }
}
