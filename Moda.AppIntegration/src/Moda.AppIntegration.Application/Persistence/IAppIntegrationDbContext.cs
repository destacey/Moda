using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Persistence;
public interface IAppIntegrationDbContext : IModaDbContext
{
    DbSet<Connector> Connectors { get; }
    DbSet<AzureDevOpsBoardsConnector> AzureDevOpsBoardsConnectors { get; }
}
