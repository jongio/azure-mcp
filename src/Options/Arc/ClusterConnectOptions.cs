using System.Text.Json.Serialization;

namespace AzureMcp.Options.Arc;

public class ClusterConnectOptions : BaseClusterOptions
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }
}