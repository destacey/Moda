using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Wayd.Organization.Domain.Models;

public sealed class TeamMemberRole : BaseSoftDeletableEntity, IHasIdAndKey
{
    private TeamMemberRole() { }

    private TeamMemberRole(string name)
    {
        Name = name;
    }

    public int Key { get; private init; }

    public string Name
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = null!;

    public bool IsActive { get; private set; } = true;

    public Result Update(string name)
    {
        try
        {
            Name = name;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result Activate()
    {
        IsActive = true;
        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }

    public static Result<TeamMemberRole> Create(string name)
    {
        try
        {
            return Result.Success(new TeamMemberRole(name));
        }
        catch (Exception ex)
        {
            return Result.Failure<TeamMemberRole>(ex.ToString());
        }
    }
}
