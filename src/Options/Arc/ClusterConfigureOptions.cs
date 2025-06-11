// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;

namespace AzureMcp.Options.Arc;

public class ClusterConfigureOptions : BaseClusterOptions
{
    [JsonPropertyName(OptionDefinitions.Arc.ConfigurationPath)]
    public string? ConfigurationPath { get; set; }
}
