using Moda.Planning.Application.ProgramIncrements.Commands;

namespace Moda.Web.Api.Models.Planning.ProgramIncrements;

public sealed record ManageProgramIncrementTeamsRequest
{
    public Guid Id { get; set; }

    public IEnumerable<Guid> TeamIds { get; set; } = new List<Guid>();

    public ManageProgramIncrementTeamsCommand ToManageProgramIncrementTeamsCommand()
    {
        return new ManageProgramIncrementTeamsCommand(Id, TeamIds);
    }
}
