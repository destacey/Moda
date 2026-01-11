using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Persistence;
public interface IAppIntegrationDbContext : IModaDbContext
{
    DbSet<Connection> Connections { get; }
    DbSet<AzureDevOpsBoardsConnection> AzureDevOpsBoardsConnections { get; }
    DbSet<AzureOpenAIConnection> AzureOpenAIConnections { get; }
}
