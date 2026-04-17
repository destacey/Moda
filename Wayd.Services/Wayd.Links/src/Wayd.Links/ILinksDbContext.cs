using Microsoft.EntityFrameworkCore;
using Wayd.Common.Application.Persistence;
using Wayd.Links.Models;

namespace Wayd.Links;

public interface ILinksDbContext : IModaDbContext
{
    DbSet<Link> Links { get; }
}
