// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Argument;
using System.Text.Json.Serialization;

namespace AzureMcp.Arguments.Search.Index;

public class IndexQueryArguments : GlobalArguments
{
    [JsonPropertyName(ArgumentDefinitions.Search.ServiceName)]
    public string? Service { get; set; }

    [JsonPropertyName(ArgumentDefinitions.Search.IndexName)]
    public string? Index { get; set; }

    [JsonPropertyName(ArgumentDefinitions.Search.QueryName)]
    public string? Query { get; set; }
}