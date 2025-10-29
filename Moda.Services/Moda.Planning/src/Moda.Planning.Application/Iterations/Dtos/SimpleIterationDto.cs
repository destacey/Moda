using Moda.Common.Domain.Enums.Planning;
using Moda.Common.Domain.Interfaces.Planning.Iterations;
using Moda.Common.Domain.Models.Planning.Iterations;
using Moda.Planning.Domain.Models.Iterations;

namespace Moda.Planning.Application.Iterations.Dtos;
public sealed record SimpleIterationDto : IMapFrom<Iteration>, ISimpleIteration
{
    public Guid Id { get; init; }
    public int Key { get; init; }
    public required string Name { get; init; }
    public IterationType Type { get; init; }
    public IterationState State { get; init; }
    public required IterationDateRange DateRange { get; init; }
    public Guid? TeamId { get; init; }
}
