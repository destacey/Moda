using Microsoft.EntityFrameworkCore;
using Wayd.Common.Application.Persistence;
using Wayd.Links.Models;

namespace Wayd.Links;

public interface ILinksDbContext : IWaydDbContext
{
    DbSet<Link> Links { get; }
}
