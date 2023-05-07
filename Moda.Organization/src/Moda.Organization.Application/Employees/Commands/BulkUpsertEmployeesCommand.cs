namespace Moda.Organization.Application.Employees.Commands;
public sealed record BulkUpsertEmployeesCommand : ICommand
{
    public BulkUpsertEmployeesCommand(IEnumerable<IExternalEmployee> employees)
    {
        // ignore records with no employee number
        Employees = employees.Where(e => !string.IsNullOrWhiteSpace(e.EmployeeNumber));
    }

    public IEnumerable<IExternalEmployee> Employees { get; }
}

public sealed class BulkUpsertEmployeesCommandValidator : CustomValidator<BulkUpsertEmployeesCommand>
{
    public BulkUpsertEmployeesCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Employees)
            .NotNull()
            .NotEmpty()
            .Must(e => e.Select(emp => emp.EmployeeNumber).Distinct().Count() == e.Count())
                .WithMessage("EmployeeNumber must be unique.");

        RuleForEach(e => e.Employees)
            .NotNull()
            .SetValidator(new ExternalEmployeeValidator());
    }
}

internal sealed class BulkUpsertEmployeesCommandHandler : ICommandHandler<BulkUpsertEmployeesCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<BulkUpsertEmployeesCommandHandler> _logger;

    public BulkUpsertEmployeesCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<BulkUpsertEmployeesCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result> Handle(BulkUpsertEmployeesCommand request, CancellationToken cancellationToken)
    {
        string requestName = request.GetType().Name;
        Dictionary<string, string> errors = new();
        Dictionary<string, string> missingManagers = new();
        List<Employee> employees = await _organizationDbContext.Employees.ToListAsync(cancellationToken) ?? new();

        foreach (var externalEmployee in request.Employees)
        {
            try
            {
                var managerId = GetManagerId(externalEmployee.ManagerEmployeeNumber);

                if (employees.FirstOrDefault(e => e.EmployeeNumber == externalEmployee.EmployeeNumber) is Employee employee)
                { // update
                    var updateResult = employee.Update(
                        externalEmployee.Name,
                        externalEmployee.EmployeeNumber,
                        externalEmployee.HireDate,
                        externalEmployee.Email,
                        externalEmployee.JobTitle,
                        externalEmployee.Department,
                        externalEmployee.OfficeLocation,
                        managerId,
                        externalEmployee.IsActive,
                        _dateTimeService.Now
                        );

                    if (updateResult.IsFailure)
                    {
                        // Reset the entity
                        await _organizationDbContext.Entry(employee).ReloadAsync(cancellationToken);
                        employee.ClearDomainEvents();

                        _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                        errors.Add(externalEmployee.EmployeeNumber, updateResult.Error);

                        continue;
                    }
                }
                else
                { // create
                    var newEmployee = Employee.Create(
                        externalEmployee.Name,
                        externalEmployee.EmployeeNumber,
                        externalEmployee.HireDate,
                        externalEmployee.Email,
                        externalEmployee.JobTitle,
                        externalEmployee.Department,
                        externalEmployee.OfficeLocation,
                        managerId,
                        _dateTimeService.Now
                        );

                    await _organizationDbContext.Employees.AddAsync(newEmployee);
                }

                // check only when no errors on update or create
                if (managerId is null && externalEmployee.ManagerEmployeeNumber is not null)
                {
                    missingManagers.Add(externalEmployee.EmployeeNumber, externalEmployee.ManagerEmployeeNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            }
        }

        try
        {
            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            await SetMissingManagers();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request} while updating the database.", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }

        //// LOCAL FUNCTIONS
        Guid? GetManagerId(string? managerEmployeeNumber)
        {
            if (string.IsNullOrWhiteSpace(managerEmployeeNumber))
                return null;

            var managerId = employees?.FirstOrDefault(e => e.EmployeeNumber == managerEmployeeNumber)?.Id;
            return managerId.IsNullEmptyOrDefault() ? null : managerId;
        }

        async Task SetMissingManagers()
        {
            if (missingManagers.Any())
            {
                List<Employee> updatedEmployees = await _organizationDbContext.Employees.ToListAsync(cancellationToken);
                foreach (var item in missingManagers)
                {
                    var managerId = GetManagerId(item.Value);
                    var employee = updatedEmployees.First(e => e.EmployeeNumber == item.Key);
                    employee.UpdateManagerId(managerId, _dateTimeService.Now);
                }

                await _organizationDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
