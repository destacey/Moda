namespace Moda.Planning.Domain.Models;
public class ProgramIncrementTeam
{
    private ProgramIncrementTeam() { }
    internal ProgramIncrementTeam(Guid programIncrementId, Guid teamId)
    {
        ProgramIncrementId = programIncrementId;
        TeamId = teamId;
    }

    public Guid ProgramIncrementId { get; private set; }
    public Guid TeamId { get; private set; }
}
