namespace Moda.Organization.Application.Interfaces;

public interface IExternalEmployeeDirectoryService
{
    Task<Result<IEnumerable<IExternalEmployee>>> GetEmployees(CancellationToken cancellationToken);
}
