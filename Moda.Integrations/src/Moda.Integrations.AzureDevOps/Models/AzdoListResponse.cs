﻿using System.Text.Json.Serialization;

namespace Moda.Integrations.AzureDevOps.Models;
internal class AzdoListResponse<T>
{
    public int Count { get; set; }

    [JsonPropertyName("value")]
    public List<T> Items { get; set; } = new();
}
