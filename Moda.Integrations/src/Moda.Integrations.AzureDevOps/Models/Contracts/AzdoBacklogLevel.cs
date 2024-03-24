using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoBacklogLevel : IExternalBacklogLevel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public BacklogCategory BacklogCategory { get; set; }
    public int Rank { get; set; }
}
