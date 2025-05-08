// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Argument;

namespace AzureMcp.Arguments.Extension;

public class BicepArguments : GlobalArguments
{
    [JsonPropertyName(ArgumentDefinitions.Extension.Bicep.CommandName)]
    public string? Command { get; set; }
}
