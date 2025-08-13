// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Options;

namespace AzureMcp.Options.Arc;

public class ArcConnectOptions : SubscriptionOptions
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("clusterName")]
    public string? ClusterName { get; set; }

    [JsonPropertyName("userProvidedPath")]
    public string? UserProvidedPath { get; set; }

    [JsonPropertyName("kubeConfigPath")]
    public string? KubeConfigPath { get; set; }
}
