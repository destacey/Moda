namespace Moda.Organization.Application.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly ILogger<EmployeeService> _logger;
    private readonly IExternalEmployeeDirectoryService _externalEmployeeDirectoryService;
    private readonly ISender _sender;

    public EmployeeService(ILogger<EmployeeService> logger, IExternalEmployeeDirectoryService externalEmployeeDirectoryService, ISender sender)
    {
        _logger = logger;
        _externalEmployeeDirectoryService = externalEmployeeDirectoryService;
        _sender = sender;
    }

    public async Task<Result> SyncExternalEmployees(CancellationToken cancellationToken)
    {
        try
        {
            // TODO delete - this is for testing because the job runs so fast
            await Task.Delay(10 * 1000);

            var getEmployeesResult = await _externalEmployeeDirectoryService.GetEmployees(cancellationToken);

            if (getEmployeesResult.IsSuccess)
            {
                if (!getEmployeesResult.Value.NotNullAndAny())
                {
                    string message = "No employees where returned by the ExternalEmployeeDirectoryService.";
                    _logger.LogError(message);
                    return Result.Failure(message);
                }

                return await _sender.Send(new BulkUpsertEmployeesCommand(getEmployeesResult.Value), cancellationToken);
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
