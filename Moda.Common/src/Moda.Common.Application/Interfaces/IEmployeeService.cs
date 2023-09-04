namespace Moda.Common.Application.Interfaces;

public interface IEmployeeService : ITransientService
{
    Task<Result> SyncExternalEmployees(CancellationToken cancellationToken);
}