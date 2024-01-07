using Moda.Common.Application.Persistence;
using Moda.Common.Domain.Employees;

namespace Moda.Common.Application.Employees.Commands;
public sealed record RemoveInvalidEmployeeCommand(Guid Id) : ICommand<int>;

internal sealed class RemoveInvalidEmployeeCommandHandler : ICommandHandler<RemoveInvalidEmployeeCommand, int>
{
    private readonly IModaDbContext _modaDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<RemoveInvalidEmployeeCommandHandler> _logger;

    public RemoveInvalidEmployeeCommandHandler(IModaDbContext modaDbContext, IDateTimeProvider dateTimeProvider, ILogger<RemoveInvalidEmployeeCommandHandler> logger)
    {
        _modaDbContext = modaDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RemoveInvalidEmployeeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _modaDbContext.Employees
                .Include(e => e.DirectReports)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (employee is null || string.IsNullOrWhiteSpace(employee.EmployeeNumber))
                return Result.Failure<int>("Employee not found.");

            var objectId = employee.EmployeeNumber;

            var updateResult = employee.Update(
                employee.Name,
                employee.EmployeeNumber,
                employee.HireDate,
                employee.Email,
                employee.JobTitle,
                employee.Department,
                employee.OfficeLocation,
                null,
                false,  // this command should not change IsActive
                _dateTimeProvider.Now
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _modaDbContext.Entry(employee).ReloadAsync(cancellationToken);
                employee.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

            foreach (var report in employee.DirectReports)
            {
                report.UpdateManagerId(null, _dateTimeProvider.Now);
            }

            await _modaDbContext.SaveChangesAsync(cancellationToken);

            _modaDbContext.Employees.Remove(employee);

            _modaDbContext.ExternalEmployeeBlacklistItems.Add(new ExternalEmployeeBlacklistItem { ObjectId = objectId });
            await _modaDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("The invalid employee {EmployeeId} with employee number {EmployeeNumber} has been deleted", request.Id, objectId);
            _logger.LogInformation("Object Id {EmployeeNumber} has been added to the ExternalEmployeeBlacklistItems list.", objectId);

            return Result.Success(employee.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

