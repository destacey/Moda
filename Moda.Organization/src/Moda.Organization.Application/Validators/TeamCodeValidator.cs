using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.Validators;
public sealed class TeamCodeValidator : CustomValidator<TeamCode>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public TeamCodeValidator(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Value)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(10)
            .MustAsync(BeUniqueTeamCode).WithMessage("The Team code already exists.");
    }

    public async Task<bool> BeUniqueTeamCode(string code, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.Teams.AllAsync(x => x.Code != code, cancellationToken);
    }
}
