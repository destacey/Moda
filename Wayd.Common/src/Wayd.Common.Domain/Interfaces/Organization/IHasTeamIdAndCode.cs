using Wayd.Common.Domain.Models.Organizations;

namespace Wayd.Common.Domain.Interfaces.Organization;

public interface IHasTeamIdAndCode
{
    Guid Id { get; }
    TeamCode Code { get; }
}
