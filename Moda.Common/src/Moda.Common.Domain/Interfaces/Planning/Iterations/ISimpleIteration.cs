using Moda.Common.Domain.Enums.Planning;
using Moda.Common.Domain.Models.Planning.Iterations;

namespace Moda.Common.Domain.Interfaces.Planning.Iterations;
public interface ISimpleIteration
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    IterationType Type { get; }
    IterationState State { get; }
    IterationDateRange DateRange { get; }
    Guid? TeamId { get; }

    // excludes OwnershipInfo and ExternalMetadata
}
