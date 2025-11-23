using Moda.Common.Domain.Models.Organizations;

namespace Moda.Common.Domain.Interfaces.Organization;

public interface IHasTeamIdAndCode
{
    Guid Id { get; }
    TeamCode Code { get; }
}
