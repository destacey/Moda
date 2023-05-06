namespace Moda.Organization.Application.Validators;
public sealed class TeamCodeValidator : CustomValidator<TeamCode>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly Guid? _id;

    public TeamCodeValidator(IOrganizationDbContext organizationDbContext, Guid? id = null)
    {
        _organizationDbContext = organizationDbContext;
        _id = id;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Value)
            .MinimumLength(2)
            .MaximumLength(10)
            .MustAsync(BeUniqueTeamCode).WithMessage("The Team code already exists.");
    }

    public async Task<bool> BeUniqueTeamCode(string code, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.BaseTeams.AsQueryable();

        if (_id.HasValue)
        {
            query = query.Where(t => t.Id != _id.Value);
        }

        return await query.AllAsync(x => x.Code != code, cancellationToken);
    }
}
