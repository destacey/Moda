using FluentValidation.AspNetCore;
using MediatR;
using Moda.Organization.Application.Persistence;

namespace Moda.Web.Api.Controllers.Organizations;
public class EmployeesController : VersionNeutralApiController
{
    private readonly ILogger<EmployeesController> _logger;
    private readonly ISender _sender;

    public EmployeesController(ILogger<EmployeesController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Employees)]
    [OpenApiOperation("Get a list of all employees.", "")]
    //[ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Get))] // TODO not working
    public async Task<ActionResult<IReadOnlyList<EmployeeListDto>>> GetList(CancellationToken cancellationToken, bool includeDisabled = false)
    {
        var employees = await _sender.Send(new GetEmployeesQuery(includeDisabled), cancellationToken);
        return Ok(employees.OrderBy(e => e.LastName));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Employees)]
    [OpenApiOperation("Get employee details using the localId.", "")]
    //[ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Get))] // TODO not working
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDetailsDto>> GetById(int id)
    {
        var employee = await _sender.Send(new GetEmployeeQuery(id));

        return employee is not null
            ? Ok(employee)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Employees)]
    [OpenApiOperation("Create an employee.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult> CreateEmployee(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateEmployeeCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.Employees)]
    //[OpenApiOperation("Delete an employee.", "")]
    //public async Task<string> Delete(string id)
    //{
    //    throw new NotImplementedException();
    //}
}
