using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Persistence;
using Moda.Links.Models;

namespace Moda.Links;
public interface ILinksDbContext : IModaDbContext
{
    DbSet<Link> Links { get; }
}
