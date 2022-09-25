namespace Moda.Common.Application.Persistence;

public interface IConnectionStringValidator
{
    bool TryValidate(string connectionString, string? dbProvider = null);
}