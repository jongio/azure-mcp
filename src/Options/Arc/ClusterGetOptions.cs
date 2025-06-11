// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace AzureMcp.Options.Arc;

public sealed class ClusterGetOptions : BaseClusterOptions
{
    [JsonPropertyName("detailed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Detailed { get; set; }
}
