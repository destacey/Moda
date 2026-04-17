using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Wayd.Infrastructure.Auditing;

public class AuditService(WaydDbContext context) : IAuditService
{
    private readonly WaydDbContext _context = context;

    public async Task<List<AuditDto>> GetUserTrailsAsync(string userId)
    {
        var trails = await _context.AuditTrails
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.DateTime)
            .Take(250)
            .ToListAsync();

        return trails.Adapt<List<AuditDto>>();
    }
}