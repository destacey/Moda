using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Application.Models;
using Moda.Common.Domain.Enums.Planning;
using Moda.Integrations.AzureDevOps.Models.Contracts;
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
    public static AzdoIteration ToAzdoIteration(this IterationDto iteration, Instant now)
    {
        AzdoIterationMetadata metadata = new()
        {
            Identifier = iteration.Identifier,
            Path = iteration.Path,
        };

        var type = iteration.HasChildren ? IterationType.Iteration : IterationType.Sprint;
        Instant? start = iteration.StartDate.HasValue ? Instant.FromDateTimeUtc(iteration.StartDate.Value) : null;
        Instant? end = iteration.EndDate.HasValue ? Instant.FromDateTimeUtc(iteration.EndDate.Value) : null;

        return new AzdoIteration(iteration.Id, iteration.Name, type, start, end, iteration.TeamId, metadata, now);
    }

    public static List<IExternalIteration<AzdoIterationMetadata>> ToIExternalIterations(this List<IterationDto> iterations, Instant now)
    {
        List<IExternalIteration<AzdoIterationMetadata>> azdoIterations = new (iterations.Count);
        foreach (var iteration in iterations)
        {
            azdoIterations.Add(iteration.ToAzdoIteration(now));
        }
        return azdoIterations;
    }
}
