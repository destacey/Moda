using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moda.Common.Domain.Data;

namespace Moda.Work.Domain.Models;

public class WorkStatus : BaseAuditableEntity<Guid>
{
    private WorkStatus() { }

    public WorkStatus(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>
    /// The name of the work status.
    /// </summary>
    public string Name { get; } = null!;

    /// <summary>
    /// The description of the work status.
    /// </summary>
    public string? Description { get; }
}
