// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Models.Sql;
using AzureMcp.Options;

namespace AzureMcp.Services.Interfaces;

/// <summary>
/// Interface for SQL service operations.
/// </summary>
public interface ISqlService
{
    /// <summary>
    /// Gets index recommendations for a SQL database.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="server">The server name.</param>
    /// <param name="resourceGroup">The resource group.</param>
    /// <param name="tableName">Optional table name to filter recommendations.</param>
    /// <param name="minImpact">Optional minimum impact threshold for recommendations.</param>
    /// <param name="subscription">The subscription ID.</param>
    /// <param name="retryPolicy">Optional retry policy configuration.</param>
    /// <returns>A list of SQL index recommendations.</returns>
    Task<List<SqlIndexRecommendation>> GetIndexRecommendationsAsync(
        string database,
        string server,
        string resourceGroup,
        string? tableName,
        int? minImpact,
        string subscription,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListServers(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListDatabases(
        string server,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);
} 