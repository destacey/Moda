using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moda.Common.Domain.Data;

namespace Moda.Work.Domain.Models;

public class WorkStatus : BaseAuditableEntity<Guid>
{
    public WorkStatus(string name, string? description)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    /// <summary>
    /// The name of the work status.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of the work status.
    /// </summary>
    public string? Description { get; }
}
