using Wayd.Common.Domain.Interfaces.ProjectPortfolioManagement;
using Wayd.Common.Domain.Models.ProjectPortfolioManagement;

namespace Wayd.Work.Domain.Models;

/// <summary>
/// A copy of the Wayd.Common.Domain.Interfaces.Ppm.ISimpleProject interface.  Used to hold basic project information for the work service and db context.
/// </summary>
public sealed class WorkProject : ISimpleProject, IHasIdAndKey<ProjectKey>
{
    private WorkProject() { }

    public WorkProject(ISimpleProject project)
    {
        Id = project.Id;
        Key = project.Key;
        Name = project.Name;
        Description = project.Description;
    }

    public Guid Id { get; private set; }
    public ProjectKey Key { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    /// <summary>
    /// Updates the specified project basic details from a PPM ISimpleProject.
    /// </summary>
    /// <param name="project"></param>
    public void UpdateDetails(ISimpleProject project)
    {
        if (project.Id != Id)
        {
            throw new ArgumentException("Project ID mismatch when updating WorkProject details.", nameof(project));
        }

        Key = project.Key;
        Name = project.Name;
        Description = project.Description;
    }
}
