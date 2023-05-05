using Moda.Common.Application.Identity.Users;

namespace Moda.Organization.Application.Services;

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

            if (getEmployeesResult.IsSuccess)
            {
                if (!getEmployeesResult.Value.NotNullAndAny())
                {
                    string message = "No employees where returned by the ExternalEmployeeDirectoryService.";
                    _logger.LogError(message);
                    return Result.Failure(message);
                }

                var upsertResult = await _sender.Send(new BulkUpsertEmployeesCommand(getEmployeesResult.Value), cancellationToken);
                if (upsertResult.IsFailure)
                    return upsertResult;

                var userUpdateResult = await _userService.UpdateMissingEmployeeIds(cancellationToken);
                if (upsertResult.IsFailure)
                    return upsertResult;

                return Result.Success();
            }
            else
            {
                string message = "Unable to retrieve external employees.";
                _logger.LogError(message);
                return Result.Failure(message);
            }
        }
        catch (Exception ex)
        {
            string message = "An error occurred while trying to sync external employees.";
            _logger.LogError(ex, message);
            return Result.Failure(message);
        }
    }
}
