using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Moda.Work.Domain.Models;

public class WorkItemField : BaseAuditableEntity<Guid>, IActivatable, IAggregateRoot
{
    public bool IsActive => throw new NotImplementedException();

    public Result Activate()
    {
        throw new NotImplementedException();
    }

    public Result Deactivate()
    {
        throw new NotImplementedException();
    }
}
