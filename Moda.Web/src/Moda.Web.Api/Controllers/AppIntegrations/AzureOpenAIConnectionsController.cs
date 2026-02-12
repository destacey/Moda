using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.AppIntegrations.Connections;

namespace Moda.Web.Api.Controllers.AppIntegrations;

[Route("api/app-integrations/connections/azure-openai")]
[ApiVersionNeutral]
[ApiController]
public class AzureOpenAIConnectionsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpPost("test")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Connections)]
    [OpenApiOperation("Test Azure OpenAI connection configuration.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> TestConfig(TestAzureOpenAIConnectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BaseUrl) || string.IsNullOrWhiteSpace(request.ApiKey))
            return BadRequest(ProblemDetailsExtensions.ForBadRequest("BaseUrl and ApiKey required.", HttpContext));

        // TODO: Implement IAzureOpenAIService to test connection
        // For now, return success if basic validation passes
        return await Task.FromResult(NoContent());

        /* Implementation when service is available:
        var result = await azureOpenAIService.TestConnection(request.BaseUrl, request.ApiKey, request.DeploymentName);
        return result.IsSuccess ? NoContent() : BadRequest(result.ToBadRequestObject(HttpContext));
        */
    }
}
