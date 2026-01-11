using Moda.Common.Application.Persistence;
using Moda.Common.Application.Validators;
using Moda.Common.Domain.Employees;

namespace Moda.Common.Application.Employees.Commands;

public sealed record BulkUpsertEmployeesCommand : ICommand, ILongRunningRequest
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
            .SetValidator(new IExternalEmployeeValidator());
    }
}

internal sealed class BulkUpsertEmployeesCommandHandler(IModaDbContext modaDbContext, IDateTimeProvider dateTimeProvider, ILogger<BulkUpsertEmployeesCommandHandler> logger) : ICommandHandler<BulkUpsertEmployeesCommand>
{
    private readonly IModaDbContext _modaDbContext = modaDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<BulkUpsertEmployeesCommandHandler> _logger = logger;

    public async Task<Result> Handle(BulkUpsertEmployeesCommand request, CancellationToken cancellationToken)
    {
        string requestName = request.GetType().Name;
        Dictionary<string, string> errors = [];
        Dictionary<string, string> missingManagers = [];
        List<Employee> employees = await _modaDbContext.Employees.ToListAsync(cancellationToken) ?? [];
        var blacklist = await _modaDbContext.ExternalEmployeeBlacklistItems.Select(b => b.ObjectId).ToListAsync(cancellationToken);

        // Build a case-insensitive lookup for manager ids from the initially loaded employees
        var employeeNumberToId = employees.ToDictionary(e => e.EmployeeNumber, e => e.Id, StringComparer.OrdinalIgnoreCase);
        var requestedEmployeeNumbers = request.Employees
            .Where(e => !blacklist.Contains(e.EmployeeNumber))
            .Select(e => e.EmployeeNumber)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var externalEmployee in request.Employees.Where(e => !blacklist.Contains(e.EmployeeNumber)))
        {
            try
            {
                var managerId = GetManagerId(externalEmployee.ManagerEmployeeNumber, employeeNumberToId);

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

                        _logger.LogError("Moda Request: Failure for Request {Name}.  Error message: {Error}", requestName, updateResult.Error);
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
                        externalEmployee.IsActive,
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
                _logger.LogError(ex, "Moda Request: Exception for Request {Name}", requestName);
            }
        }

        try
        {
            await _modaDbContext.SaveChangesAsync(cancellationToken);

            await ProcessMissingManagers(missingManagers, cancellationToken);

            await DeactivateEmployeesNotInPayload(requestedEmployeeNumbers, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} while updating the database.", requestName);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }

    private async Task ProcessMissingManagers(Dictionary<string, string> missingManagers, CancellationToken cancellationToken)
    {
        if (missingManagers.Count == 0)
            return;

        // Build sets to limit queries to only affected employees and managers
        var employeeNumbersNeedingManagers = missingManagers.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var managerNumbersNeeded = missingManagers.Values.Where(v => !string.IsNullOrWhiteSpace(v)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Load employees that need manager updates into a dictionary for O(1) lookups
        var employeesNeedingUpdate = await _modaDbContext.Employees
            .Where(e => employeeNumbersNeedingManagers.Contains(e.EmployeeNumber))
            .ToDictionaryAsync(e => e.EmployeeNumber, e => e, StringComparer.OrdinalIgnoreCase, cancellationToken);

        if (employeesNeedingUpdate.Count == 0)
            return;

        // Load managers referenced directly into a dictionary
        var managerLookup = await _modaDbContext.Employees
            .Where(e => managerNumbersNeeded.Contains(e.EmployeeNumber))
            .ToDictionaryAsync(e => e.EmployeeNumber, e => e.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        // Update tracked employee entities with resolved manager ids
        foreach (var kvp in missingManagers)
        {
            if (!employeesNeedingUpdate.TryGetValue(kvp.Key, out var employee))
                continue;

            if (!managerLookup.TryGetValue(kvp.Value, out var managerId))
                continue;

            employee.UpdateManagerId(managerId, _dateTimeProvider.Now);
        }

        await _modaDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DeactivateEmployeesNotInPayload(HashSet<string> requestedEmployeeNumbers, CancellationToken cancellationToken)
    {
        // Find active employees not included in the request payload and deactivate them
        var toDeactivate = await _modaDbContext.Employees
            .Where(e => e.IsActive && !requestedEmployeeNumbers.Contains(e.EmployeeNumber))
            .ToListAsync(cancellationToken);

        if (toDeactivate.Count == 0)
            return;

        foreach (var employee in toDeactivate)
        {
            var result = employee.Deactivate(_dateTimeProvider.Now);
            if (result.IsFailure)
            {
                _logger.LogError("Failed to deactivate employee {EmployeeNumber}. Error: {Error}", employee.EmployeeNumber, result.Error);
            }
        }

        await _modaDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated {Count} employees not present in the payload.", toDeactivate.Count);
    }

    private static Guid? GetManagerId(string? managerEmployeeNumber, IDictionary<string, Guid> employeeNumberToId)
    {
        if (string.IsNullOrWhiteSpace(managerEmployeeNumber) || employeeNumberToId.Count == 0)
            return null;

        return employeeNumberToId.TryGetValue(managerEmployeeNumber, out var id) && id != Guid.Empty
            ? id
            : null;
    }
}
