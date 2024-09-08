using Moda.Common.Application.Models;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record CreateRoadmapReponse
{
    public required ObjectIdAndKey RoadmapIds { get; set; }

    /// <summary>
    /// The result of linking the Roadmap to the parent. If the action was successful, this will be null.  If a parentId was not provided in the request, this will be null.
    /// </summary>
    public string? LinkToParentError { get; set; }
}
