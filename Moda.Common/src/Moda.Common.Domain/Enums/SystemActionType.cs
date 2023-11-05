using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums;
public enum SystemActionType
{
    [Display(Name = "Service Data Replication", Description = "An event where a service replicates data from another service.", Order = 1)]
    ServiceDataReplication = 1,
}
