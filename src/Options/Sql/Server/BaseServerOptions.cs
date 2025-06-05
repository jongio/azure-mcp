// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;

namespace AzureMcp.Options.Sql.Server;

/// <summary>
/// Base options for SQL Server operations that require a server name.
/// </summary>
public class BaseServerOptions : BaseSqlOptions
{
    /// <summary>
    /// The name of the Azure SQL Server.
    /// </summary>
    [JsonPropertyName("server-name")]
    public string? Server { get; set; }
}
