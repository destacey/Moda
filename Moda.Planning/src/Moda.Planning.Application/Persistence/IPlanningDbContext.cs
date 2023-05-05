using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Persistence;
using Moda.Planning.Domain.Models;

namespace Moda.Planning.Application.Persistence;
public interface IPlanningDbContext : IModaDbContext
{
    DbSet<ProgramIncrement> ProgramIncrements { get; }
}
