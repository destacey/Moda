using CSharpFunctionalExtensions;

namespace Moda.Common.Application.Interfaces;

public interface IExternalEmployeeDirectoryService
{
    Task<Result<IEnumerable<IExternalEmployee>>> GetEmployees(CancellationToken cancellationToken);
}
