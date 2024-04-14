using System.Runtime.Serialization;

namespace Moda.Integrations.AzureDevOps.Models.WorkItems;

[DataContract]
internal sealed record WorkItemsBatchRequest
{
    [DataMember]
    public string[] Fields { get; set; } = [];

    [DataMember]
    public int[] Ids { get; set; } = [];

    public static WorkItemsBatchRequest Create(IEnumerable<int> ids, IEnumerable<string> fields)
    {
        return new WorkItemsBatchRequest
        {
            Ids = ids.ToArray(),
            Fields = fields.ToArray()
        };
    }
}
