using MediatR;
using Moda.Common.Application.Employees.Commands;
using Moda.Common.Application.Identity.Users;

namespace Moda.Common.Application.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly ILogger<EmployeeService> _logger;
    private readonly IExternalEmployeeDirectoryService _externalEmployeeDirectoryService;
    private readonly ISender _sender;
    private readonly IUserService _userService;

    public EmployeeService(ILogger<EmployeeService> logger, IExternalEmployeeDirectoryService externalEmployeeDirectoryService, ISender sender, IUserService userService)
    {
        _logger = logger;
        _externalEmployeeDirectoryService = externalEmployeeDirectoryService;
        _sender = sender;
        _userService = userService;
    }

    public async Task<Result> SyncExternalEmployees(CancellationToken cancellationToken)
    {
        try
        {
            var getEmployeesResult = await _externalEmployeeDirectoryService.GetEmployees(cancellationToken);
            if (getEmployeesResult.IsFailure)
            {
                string message = "Unable to retrieve external employees.";
                _logger.LogError(message);
                return Result.Failure(message);
            }

            var employees = getEmployeesResult.Value.ToList();
            _logger.LogDebug("Retrieved {Count} employees from the ExternalEmployeeDirectoryService.", employees.Count);

            if (employees.Count == 0)
            {
                string message = "No employees where returned by the ExternalEmployeeDirectoryService.";
                _logger.LogWarning(message);
                return Result.Failure(message);
            }

            var upsertResult = await _sender.Send(new BulkUpsertEmployeesCommand(employees), cancellationToken);
            if (upsertResult.IsFailure)
                return upsertResult;

            var userLinkResult = await _userService.UpdateMissingEmployeeIds(cancellationToken);
            if (userLinkResult.IsFailure)
                return upsertResult;

            var userUpdateResult = await _userService.SyncUsersFromEmployeeRecords(employees, cancellationToken);
            if (userUpdateResult.IsFailure)
                return upsertResult;

            return Result.Success();
        }
        catch (Exception ex)
        {
            string message = "An error occurred while trying to sync external employees.";
            _logger.LogError(ex, message);
            return Result.Failure(message);
        }
    }
}
