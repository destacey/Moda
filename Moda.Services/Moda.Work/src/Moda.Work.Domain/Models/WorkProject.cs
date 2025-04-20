using Moda.Common.Domain.Interfaces.Ppm;

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
    }

    public Guid Id { get; private set; }
    public int Key { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; }

    /// <summary>
    /// Updates the specified project from a PPM ISimpleProject.
    /// </summary>
    /// <param name="project"></param>
    public void Update(ISimpleProject project)
    {
        Id = project.Id;
        Key = project.Key;
        Name = project.Name;
    }
}
