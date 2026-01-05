using Moda.Common.Domain.Models.ProjectPortfolioManagement;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Validators;

public sealed class ProjectKeyValidator : CustomValidator<ProjectKey>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext;
    private readonly Guid? _id;

    public ProjectKeyValidator(IProjectPortfolioManagementDbContext ppmDbContext, Guid? id = null)
    {
        _ppmDbContext = ppmDbContext;
        _id = id;

        RuleFor(x => x.Value)
            .NotEqual("TTT").WithMessage("The project key 'TTT' is reserved and cannot be used.")
            .MustAsync(BeUniqueProjectKey).OverridePropertyName(string.Empty).WithMessage("The project key already exists.");
    }

    private async Task<bool> BeUniqueProjectKey(string key, CancellationToken cancellationToken)
    {
        var query = _ppmDbContext.Projects.AsQueryable();
        
        if (_id.HasValue)
        {
            query = query.Where(p => p.Id != _id.Value);
        }

        return await query.AllAsync(x => x.Key != key, cancellationToken);
    }
}
