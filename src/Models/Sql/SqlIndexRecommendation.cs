// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Models.Sql;

/// <summary>
/// Represents an index recommendation for a SQL database.
/// </summary>
public sealed record SqlIndexRecommendation
{
    /// <summary>
    /// Gets or sets the name of the recommended index.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the recommended index.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated impact score of implementing this recommendation.
    /// </summary>
    public int Impact { get; init; }

    /// <summary>
    /// Gets or sets the table name for which this index is recommended.
    /// </summary>
    public string TableName { get; init; } = string.Empty;
}
