namespace Moda.Web.Api.Extensions;

/// <summary>
/// Extension methods for ASP.NET Core controllers.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Creates an OK response with polymorphic serialization support.
    /// Sets the DeclaredType to force System.Text.Json to include the $type discriminator
    /// in the response for types decorated with [JsonDerivedType] attributes.
    /// </summary>
    /// <typeparam name="T">The base type of the polymorphic response</typeparam>
    /// <param name="controller">The controller instance</param>
    /// <param name="value">The value to return</param>
    /// <returns>An ActionResult with proper type discrimination for JSON serialization</returns>
    public static ActionResult<T> OkPolymorphic<T>(this ControllerBase controller, T? value) where T : class
    {
        if (value is null)
            return controller.NotFound();

        return new ObjectResult(value)
        {
            DeclaredType = typeof(T),
            StatusCode = StatusCodes.Status200OK
        };
    }
}
