// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AzureMcp.Models.Arc;
using AzureMcp.Options.Arc;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Commands.Arc;

[JsonSerializable(typeof(ClusterConfigureCommand.ClusterConfigureCommandResult))]
[JsonSerializable(typeof(ClusterListCommand.ClusterListCommandResult))]
[JsonSerializable(typeof(ClusterGetCommand.ClusterGetCommandResult))]
[JsonSerializable(typeof(ConfigurationResult))]
[JsonSerializable(typeof(Cluster))]
[JsonSerializable(typeof(List<Cluster>))]
[JsonSerializable(typeof(ClusterConfigureOptions))]
[JsonSerializable(typeof(ClusterListOptions))]
[JsonSerializable(typeof(ClusterGetOptions))]
[JsonSerializable(typeof(BaseClusterOptions))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ArcJsonContext : JsonSerializerContext
{
}
