using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.Planning;
using Moda.Integrations.AzureDevOps.Models.Contracts;
using Moda.Integrations.AzureDevOps.Utils;
using NodaTime;

namespace Moda.Integrations.AzureDevOps.Models.Projects;

internal sealed record IterationDto
{
    public int Id { get; set; }

    public Guid Identifier { get; set; }

    public required string Name { get; set; }

    public required string Path { get; set; }

    public Guid? TeamId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool HasChildren { get; set; }

    public static IterationDto FromIterationNodeResponse(IterationNodeResponse node, Guid? teamId = null)
    {
        return new IterationDto
        {
            Id = node.Id,
            Identifier = node.Identifier,
            Name = node.Name,
            Path = ClassificationNodeUtils.RemoveClassificationTypeFromPath(node.Path),
            TeamId = teamId,
            StartDate = node.Attributes?.StartDate,
            EndDate = node.Attributes?.EndDate,
            HasChildren = node.Children is not null && node.Children.Count > 0
        };
    }
}

internal static class IterationDtoExtensions
{
    public static AzdoIteration ToAzdoIteration(this IterationDto iteration, Instant now, Guid projectId)
    {
        AzdoIterationMetadata metadata = new()
        {
            ProjectId = projectId,
            Identifier = iteration.Identifier,
            Path = iteration.Path,
        };

        var type = iteration.HasChildren ? IterationType.Iteration : IterationType.Sprint;
        Instant? start = iteration.StartDate.HasValue ? Instant.FromDateTimeUtc(iteration.StartDate.Value) : null;
        Instant? end = iteration.EndDate.HasValue ? Instant.FromDateTimeUtc(iteration.EndDate.Value) : null;

        return new AzdoIteration(iteration.Id, iteration.Name, type, start, end, iteration.TeamId, metadata, now);
    }

    public static List<IExternalIteration<AzdoIterationMetadata>> ToIExternalIterations(this List<IterationDto> iterations, Instant now, Guid projectId)
    {
        List<IExternalIteration<AzdoIterationMetadata>> azdoIterations = new (iterations.Count);
        foreach (var iteration in iterations)
        {
            azdoIterations.Add(iteration.ToAzdoIteration(now, projectId));
        }
        return azdoIterations;
    }
}
