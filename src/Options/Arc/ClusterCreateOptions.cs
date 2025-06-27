using System.Text.Json.Serialization;

namespace AzureMcp.Options.Arc;

public class ClusterCreateOptions : BaseClusterOptions
{
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("tenant")]
    public string? Tenant { get; set; }
}