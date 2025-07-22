// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Areas.ContainerApps.Commands.App;
using AzureMcp.Areas.ContainerApps.Models;

namespace AzureMcp.Areas.ContainerApps;

[JsonSerializable(typeof(List<ContainerApp>))]
[JsonSerializable(typeof(ContainerApp))]
[JsonSerializable(typeof(AppListCommand.AppListCommandResult))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
public partial class ContainerAppsSerializationContext : JsonSerializerContext
{
}
