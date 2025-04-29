// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMcp.Arguments.Search.Index;

public class IndexListArguments : GlobalArguments
{
    [JsonPropertyName(ArgumentDefinitions.Search.ServiceName)]
    public string? Service { get; set; }
}