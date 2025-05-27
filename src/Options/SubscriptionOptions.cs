// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;

namespace AzureMcp.Options;

public class SubscriptionOptions : GlobalOptions
{
    [JsonPropertyName(OptionDefinitions.Common.SubscriptionName)]
    public string? Subscription { get; set; }
}
