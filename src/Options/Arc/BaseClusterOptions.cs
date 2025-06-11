// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;

namespace AzureMcp.Options.Arc;

public class BaseClusterOptions : SubscriptionOptions
{
    [JsonPropertyName(OptionDefinitions.Arc.ClusterName)]
    public string? ClusterName { get; set; }
    [JsonPropertyName(OptionDefinitions.Common.ResourceGroupName)]
    public new string? ResourceGroup { get; set; }
}
