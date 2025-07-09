// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;
using AzureMcp.Options.Subscription;

namespace AzureMcp.Options.Arc;

public class ArcConnectOptions : BaseSubscriptionOptions
{
    [JsonPropertyName("subscriptionId")]
    public string? SubscriptionId { get; set; }

    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("resourceGroupName")]
    public string? ResourceGroupName { get; set; }

    [JsonPropertyName("clusterName")]
    public string? ClusterName { get; set; }

    [JsonPropertyName("userProvidedPath")]
    public string? UserProvidedPath { get; set; }
}
