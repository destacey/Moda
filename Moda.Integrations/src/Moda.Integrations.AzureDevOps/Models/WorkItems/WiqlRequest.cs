using System.Runtime.Serialization;

namespace Wayd.Integrations.AzureDevOps.Models.WorkItems;

[DataContract]
internal sealed record WiqlRequest
{
    [DataMember]
    public required string Query { get; set; }
}
