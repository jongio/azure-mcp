// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using AzureMcp.Models.Sql;
using AzureMcp.Options;

namespace AzureMcp.Services.Interfaces;

/// <summary>
/// Service interface for Azure Sql operations.
/// </summary>
public interface ISqlService
{
    /// <summary>
    /// Lists all Sql servers in the specified subscription.
    /// </summary>
    /// <param name="subscription">The Azure subscription ID.</param>
    /// <param name="tenant">Optional tenant ID to scope the operation.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <returns>A list of server names.</returns>
    Task<List<string>> ListServers(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Lists all databases in the specified Sql server.
    /// </summary>
    /// <param name="subscription">The Azure subscription ID.</param>
    /// <param name="resourceGroup">The resource group containing the server.</param>
    /// <param name="server">The name of the Sql server.</param>
    /// <param name="tenant">Optional tenant ID to scope the operation.</param>
    /// <param name="authMethod">Authentication method to use.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <returns>A list of database names.</returns>
    Task<List<string>> ListDatabases(
        string subscription,
        string resourceGroup,
        string server,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Gets index recommendations for an Azure Sql database table.
    /// </summary>
    /// <param name="database">The database name to get recommendations for.</param>
    /// <param name="server">The name of the Sql server containing the database.</param>
    /// <param name="tableName">Optional name of a specific table to get recommendations for.</param>
    /// <param name="minImpact">Optional minimum impact threshold.</param>
    /// <param name="subscription">Azure subscription ID containing the database.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <returns>A list of index recommendations.</returns>
    Task<List<SqlIndexRecommendation>> GetIndexRecommendationsAsync(
        string database,
        string server,
        string? tableName,
        int? minImpact,
        string subscription,
        RetryPolicyOptions? retryPolicy = null);
}
