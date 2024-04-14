using System.Runtime.Serialization;

namespace Moda.Integrations.AzureDevOps.Models.WorkItems;

[DataContract]
internal sealed record WiqlRequest
{
    [DataMember]
    public required string Query { get; set; }
}
