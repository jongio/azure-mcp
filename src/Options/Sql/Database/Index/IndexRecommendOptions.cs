// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Models.Option;
using AzureMcp.Options.Sql.Database;

namespace AzureMcp.Options.Sql.Database.Index;

/// <summary>
/// Options for getting SQL index recommendations for tables in a database.
/// </summary>
public sealed class IndexRecommendOptions : BaseDatabaseOptions
{
    /// <summary>
    /// The name of the table to get index recommendations for.
    /// </summary>
    [JsonPropertyName(OptionDefinitions.Sql.TableName)]
    public string? TableName { get; set; }

    /// <summary>
    /// The minimum impact threshold for index recommendations.
    /// </summary>
    [JsonPropertyName(OptionDefinitions.Sql.MinimumImpactName)]
    public int? MinimumImpact { get; set; }
}
