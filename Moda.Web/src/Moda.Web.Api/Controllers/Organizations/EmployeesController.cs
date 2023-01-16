﻿namespace Moda.Web.Api.Controllers.Organizations;

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
    [OpenApiOperation(nameof(GetList), "Get a list of all employees.", "")]
    //[ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Get))] // TODO not working
    public async Task<ActionResult<IReadOnlyList<EmployeeListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var employees = await _sender.Send(new GetEmployeesQuery(includeInactive), cancellationToken);
        return Ok(employees.OrderBy(e => e.LastName));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Employees)]
    [OpenApiOperation(nameof(GetById), "Get employee details using the localId.", "")]
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
    [OpenApiOperation(nameof(Create), "Create an employee.", "")]
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
    [OpenApiOperation(nameof(Update), "Update an employee.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    //[OpenApiOperation(nameof(Delete), "Delete an employee.", "")]
    //public async Task<string> Delete(string id)
    //{
    //    throw new NotImplementedException();
    //}
}
