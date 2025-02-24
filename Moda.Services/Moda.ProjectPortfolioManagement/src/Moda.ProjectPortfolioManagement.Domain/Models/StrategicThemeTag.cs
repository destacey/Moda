namespace Moda.ProjectPortfolioManagement.Domain.Models;

public class StrategicThemeTag<T> : ISystemAuditable
{
    protected StrategicThemeTag() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StrategicThemeTag"/> class.
    /// </summary>
    /// <param name="objectId"></param>
    /// <param name="strategicThemeId"></param>
    internal StrategicThemeTag(Guid objectId, Guid strategicThemeId)
    {
        ObjectId = objectId;
        StrategicThemeId = strategicThemeId;
    }

    public Guid ObjectId { get; private set; }

    public T? Object { get; private set; }

    public Guid StrategicThemeId { get; private set; }

    public StrategicTheme? StrategicTheme { get; private set; }
}
