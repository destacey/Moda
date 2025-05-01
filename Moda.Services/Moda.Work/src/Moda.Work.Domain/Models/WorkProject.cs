using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;

namespace Moda.Work.Domain.Models;

/// <summary>
/// A copy of the Moda.Common.Domain.Interfaces.Ppm.ISimpleProject interface.  Used to hold basic project information for the work service and db context.
/// </summary>
public class WorkProject : ISimpleProject, IHasIdAndKey
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
    public int Key { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    /// <summary>
    /// Updates the specified project basic details from a PPM ISimpleProject.
    /// </summary>
    /// <param name="project"></param>
    public void UpdateDetails(ISimpleProject project)
    {
        Id = project.Id;
        Key = project.Key;
        Name = project.Name;
        Description = project.Description;
    }
}
