// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;

namespace AzureMcp.Options.Storage.Blob;

public class BaseContainerOptions : BaseStorageOptions
{
    [JsonPropertyName(OptionDefinitions.Storage.ContainerName)]
    public string? Container { get; set; }
}
