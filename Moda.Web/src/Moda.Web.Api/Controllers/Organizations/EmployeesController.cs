using Moda.Common.Application.Employees.Commands;
using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Application.Employees.Queries;
using Moda.Web.Api.Models.Organizations.Employees;

namespace Moda.Web.Api.Controllers.Organizations;

[Route("api/organization/employees")]
[ApiVersionNeutral]
[ApiController]
public class EmployeesController : ControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<EmployeeListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var employees = await _sender.Send(new GetEmployeesQuery(includeInactive), cancellationToken);
        return Ok(employees.OrderBy(e => e.LastName));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Employees)]
    [OpenApiOperation("Get employee details using the localId.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
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
    public async Task<ActionResult> Create(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateEmployeeCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Employees)]
    [OpenApiOperation("Update an employee.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateEmployeeCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.Employees)]
    //[OpenApiOperation("Delete an employee.", "")]
    //public async Task<string> Delete(string id)
    //{
    //    throw new NotImplementedException();
    //}

    [HttpGet("{id}/direct-reports")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Employees)]
    [OpenApiOperation("Get a list of direct reports for an employee.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<EmployeeListDto>>> GetDirectReports(Guid id, CancellationToken cancellationToken)
    {
        var directReports = await _sender.Send(new GetDirectReportsQuery(id), cancellationToken);
        return Ok(directReports.OrderBy(e => e.LastName));
    }


    [HttpPost("{id}/remove-invalid")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Employees)]
    [OpenApiOperation("Remove invalid employee record from employee list.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveInvalid(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveInvalidEmployeeCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }
}
