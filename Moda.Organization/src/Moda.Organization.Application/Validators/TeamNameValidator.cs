using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.Validators;
public sealed class TeamNameValidator : CustomValidator<string>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly Guid? _id;

    public TeamNameValidator(IOrganizationDbContext organizationDbContext, Guid? id = null)
    {
        _organizationDbContext = organizationDbContext;
        _id = id;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(BeUniqueTeamName).WithMessage("The Team code already exists.");
    }

    public async Task<bool> BeUniqueTeamName(string name, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.BaseTeams.AsQueryable();

        if (_id.HasValue)
        {
            query = query.Where(t => t.Id != _id.Value);
        }

        return await query.AllAsync(x => x.Name != name, cancellationToken);
    }
}
