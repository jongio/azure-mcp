// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;

namespace AzureMcp.Options;

public class GlobalOptions
{
    [JsonPropertyName(OptionDefinitions.Common.ResourceGroupName)]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName(OptionDefinitions.Common.TenantName)]
    public string? Tenant { get; set; }

    [JsonPropertyName(OptionDefinitions.Common.AuthMethodName)]
    public AuthMethod? AuthMethod { get; set; }

    public RetryPolicyOptions? RetryPolicy { get; set; }
}
