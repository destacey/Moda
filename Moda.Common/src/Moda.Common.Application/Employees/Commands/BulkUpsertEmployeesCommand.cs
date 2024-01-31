using Moda.Common.Application.Persistence;
using Moda.Common.Application.Validators;
using Moda.Common.Domain.Employees;

namespace Moda.Common.Application.Employees.Commands;

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
    private readonly IModaDbContext _modaDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<BulkUpsertEmployeesCommandHandler> _logger;

    public BulkUpsertEmployeesCommandHandler(IModaDbContext modaDbContext, IDateTimeProvider dateTimeProvider, ILogger<BulkUpsertEmployeesCommandHandler> logger)
    {
        _modaDbContext = modaDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(BulkUpsertEmployeesCommand request, CancellationToken cancellationToken)
    {
        string requestName = request.GetType().Name;
        Dictionary<string, string> errors = [];
        Dictionary<string, string> missingManagers = [];
        List<Employee> employees = await _modaDbContext.Employees.ToListAsync(cancellationToken) ?? [];
        var blacklist = await _modaDbContext.ExternalEmployeeBlacklistItems.Select(b => b.ObjectId).ToListAsync(cancellationToken);

        foreach (var externalEmployee in request.Employees.Where(e => !blacklist.Contains(e.EmployeeNumber)))
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
                        _dateTimeProvider.Now
                        );

                    if (updateResult.IsFailure)
                    {
                        // Reset the entity
                        await _modaDbContext.Entry(employee).ReloadAsync(cancellationToken);
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
                        _dateTimeProvider.Now
                        );

                    await _modaDbContext.Employees.AddAsync(newEmployee, cancellationToken);
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
            await _modaDbContext.SaveChangesAsync(cancellationToken);

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
            if (missingManagers.Count != 0)
            {
                List<Employee> updatedEmployees = await _modaDbContext.Employees.ToListAsync(cancellationToken);
                foreach (var item in missingManagers)
                {
                    var managerId = GetManagerId(item.Value);
                    var employee = updatedEmployees.First(e => e.EmployeeNumber == item.Key);
                    employee.UpdateManagerId(managerId, _dateTimeProvider.Now);
                }

                await _modaDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
