using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Moda.Infrastructure.Auditing;

public class AuditService : IAuditService
{
    private readonly ModaDbContext _context;

    public AuditService(ModaDbContext context) => _context = context;

    public async Task<List<AuditDto>> GetUserTrailsAsync(Guid userId)
    {
        var trails = await _context.AuditTrails
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.DateTime)
            .Take(250)
            .ToListAsync();

        return trails.Adapt<List<AuditDto>>();
    }
}