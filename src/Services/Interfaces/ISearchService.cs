// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Arguments;

namespace AzureMcp.Services.Interfaces;

public interface ISearchService
{
    Task<List<string>> ListServices(
        string subscription,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null);

    Task<List<string>> ListIndexes(
        string serviceName,
        RetryPolicyArguments? retryPolicy = null);

    /// <summary>
    /// Gets the full definition of a search index
    /// </summary>
    /// <param name="serviceName">The name of the search service</param>
    /// <param name="indexName">The name of the search index</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <returns>The search index definition object</returns>
    Task<object> DescribeIndex(
        string serviceName,
        string indexName,
        RetryPolicyArguments? retryPolicy = null);
}