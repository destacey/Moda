namespace Moda.Organization.Application.Interfaces;

public interface IEmployeeService : ITransientService
{
    Task<Result> SyncExternalEmployees(CancellationToken cancellationToken);
}