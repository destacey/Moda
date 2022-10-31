using FluentValidation.AspNetCore;
using MediatR;
using Moda.Organization.Application.Persistence;
using Moda.Web.Api.Models.Organization;

namespace Moda.Web.Api.Controllers.Organizations;
public class EmployeesController : VersionNeutralApiController
{
    private readonly ILogger<EmployeesController> _logger;
    private readonly ISender _sender;

    // TODO can we remove this
    private readonly IOrganizationDbContext _organizationDbContext;

    public EmployeesController(ILogger<EmployeesController> logger, ISender sender, IOrganizationDbContext organizationDbContext)
    {
        _logger = logger;
        _sender = sender;
        _organizationDbContext = organizationDbContext;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Employees)]
    [OpenApiOperation("Get a list of all employees.", "")]
    public async Task<ActionResult<IReadOnlyList<EmployeeListDto>>> GetList(CancellationToken cancellationToken, bool includeDisabled = false)
    {
        var employees = await _sender.Send(new GetEmployeesQuery(includeDisabled));
        return Ok(employees.OrderBy(e => e.LastName));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Employees)]
    [OpenApiOperation("Get employee details.", "")]
    public async Task<EmployeeDetailsDto?> GetById(Guid employeeId)
    {
        return await _sender.Send(new GetEmployeeQuery(employeeId));
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Employees)]
    [OpenApiOperation("Create an employee.", "")]
    public async Task<ActionResult<string>> CreateEmployee(CreateEmployeeRequest request)
    {
        var createCommand = request.ToCreateEmployeeCommand();
        var validator = new CreateEmployeeCommandValidator(_organizationDbContext);
        var validationResult = await validator.ValidateAsync(createCommand);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return UnprocessableEntity(ModelState);
        }

        var result = await _sender.Send(createCommand);
        return result.IsSuccess
            ? CreatedAtRoute(nameof(GetById), new { id = result.Value })
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
