using System.Text.Json;
using System.Text.Json.Serialization;
using Moda.Integrations.AzureDevOps.Models.WorkItems;

namespace Moda.Integrations.AzureDevOps.Models.Converters;
internal class ReportingWorkItemLinkResponseConverter : JsonConverter<ReportingWorkItemLinkResponse>
{
    public override ReportingWorkItemLinkResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            var root = doc.RootElement;
            var attributes = root.GetProperty("attributes");

            return new ReportingWorkItemLinkResponse
            {
                Rel = root.GetProperty("rel").GetString()!,
                SourceId = attributes.GetProperty("sourceId").GetInt32(),
                TargetId = attributes.GetProperty("targetId").GetInt32(),
                ChangedDate = attributes.GetProperty("changedDate").GetDateTime(),
                ChangedBy = JsonSerializer.Deserialize<UserResponse>(attributes.GetProperty("changedBy").GetRawText(), options),
                Comment = attributes.GetProperty("comment").GetString(),
                IsActive = attributes.GetProperty("isActive").GetBoolean(),
                ChangedOperation = attributes.GetProperty("changedOperation").GetString()!,
                SourceProjectId = attributes.GetProperty("sourceProjectId").GetGuid(),
                TargetProjectId = attributes.GetProperty("targetProjectId").GetGuid()
            };
        }
    }

    public override void Write(Utf8JsonWriter writer, ReportingWorkItemLinkResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("rel", value.Rel);
        writer.WriteStartObject("attributes");
        writer.WriteNumber("sourceId", value.SourceId);
        writer.WriteNumber("targetId", value.TargetId);
        writer.WriteBoolean("isActive", value.IsActive);
        writer.WriteString("changedDate", value.ChangedDate.ToString("o"));
        writer.WritePropertyName("changedBy");
        JsonSerializer.Serialize(writer, value.ChangedBy, options);
        writer.WriteString("comment", value.Comment);
        writer.WriteString("changedOperation", value.ChangedOperation);
        writer.WriteString("sourceProjectId", value.SourceProjectId.ToString());
        writer.WriteString("targetProjectId", value.TargetProjectId.ToString());
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
