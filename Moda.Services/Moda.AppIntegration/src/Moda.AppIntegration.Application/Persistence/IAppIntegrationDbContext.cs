using Microsoft.EntityFrameworkCore;
using Moda.AppIntegration.Domain.Models.AzureOpenAI;

namespace Moda.AppIntegration.Application.Persistence;
public interface IAppIntegrationDbContext : IModaDbContext
{
    DbSet<Connection> Connections { get; }
    DbSet<AzureDevOpsBoardsConnection> AzureDevOpsBoardsConnections { get; }
    DbSet<AzureOpenAIConnection> AzureOpenAIConnections { get; }
}
