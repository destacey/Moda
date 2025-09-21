using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Models;
using Moda.Integrations.AzureDevOps.Models.Contracts;

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

    public static Func<IterationNodeDto, IterationDto> FromIterationNodeDto =>
        iterationNodeDto => new IterationDto()
        {
            Id = iterationNodeDto.Id,
            Identifier = iterationNodeDto.Identifier,
            Name = iterationNodeDto.Name,
            Path = iterationNodeDto.Path,
            TeamId = iterationNodeDto.TeamId,
            StartDate = iterationNodeDto.StartDate,
            EndDate = iterationNodeDto.EndDate,
            HasChildren = iterationNodeDto.Children is not null && iterationNodeDto.Children.Count != 0
        };
}

internal static class IterationDtoExtensions
{
    public static AzdoIteration ToAzdoSprint(this IterationDto iteration)
    {
        return new AzdoIteration
        {
            Id = iteration.Id.ToString(),
            Name = iteration.Name,
            TeamId = iteration.TeamId,
            StartDate = iteration.StartDate,
            EndDate = iteration.EndDate,
            Metadata = new AzdoIterationMetadata
            {
                Identifier = iteration.Identifier,
                Path = iteration.Path,
                HasChildren = iteration.HasChildren
            }
        };
    }

    public static List<IExternalSprint<AzdoIterationMetadata>> ToIExternalSprints(this List<IterationDto> iterations)
    {
        List<IExternalSprint<AzdoIterationMetadata>> azdoSprints = new (iterations.Count);
        foreach (var iteration in iterations)
        {
            azdoSprints.Add(iteration.ToAzdoSprint());
        }
        return azdoSprints;
    }
}
