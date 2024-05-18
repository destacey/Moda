using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalBacklogLevel
{
    string Id { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
    BacklogCategory BacklogCategory { get; }
    int Rank { get; set; }
}
