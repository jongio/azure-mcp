// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Sql;
using AzureMcp.Options;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Services.Azure.Sql;

public sealed class SqlService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    ILogger<SqlService> logger) : BaseAzureService(tenantService), ISqlService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<SqlService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string CACHE_GROUP = "sql";
    private const string SQL_SERVERS_CACHE_KEY = "servers";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

    private new void ValidateRequiredParameters(params string[] parameters)
    {
        foreach (var param in parameters)
        {
            ArgumentException.ThrowIfNullOrEmpty(param, param);
        }
    }

    public async Task<List<Models.Sql.SqlIndexRecommendation>> GetIndexRecommendationsAsync(
        string database,
        string server,
        string resourceGroup,
        string? tableName,
        int? minImpact,
        string subscription,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup, server, database);

        try
        {
            // Get subscription info and credentials
            var credential = await GetCredential();
            var subscriptionInfo = await _subscriptionService.GetSubscription(subscription);

            var clientOptions = new ArmClientOptions();
            if (retryPolicy != null)
            {
                clientOptions.Retry.MaxRetries = retryPolicy.MaxRetries;
                clientOptions.Retry.Mode = retryPolicy.Mode;
            }

            var armClient = new ArmClient(credential, subscriptionInfo.Id, clientOptions);
            var resourceGroupResource = armClient.GetResourceGroupResource(ResourceGroupResource.CreateResourceIdentifier(subscriptionInfo.Id, resourceGroup));

            // Find the server resource
            var serverResponse = await resourceGroupResource.GetSqlServerAsync(server);

            if (serverResponse?.Value == null)
            {
                throw new SqlResourceNotFoundException($"SQL Server '{server}' not found in resource group '{resourceGroup}' and subscription '{subscription}'");
            }

            var serverResource = serverResponse.Value;

            // Get database resource
            var databaseResponse = await serverResource.GetSqlDatabaseAsync(database);

            if (databaseResponse?.Value == null)
            {
                throw new SqlResourceNotFoundException($"Database '{database}' not found on server '{server}'");
            }

            var databaseResource = databaseResponse.Value;
            var recommendations = new List<Models.Sql.SqlIndexRecommendation>();

            try
            {
                // Get the collection of advisors for the database
                var advisorCollection = databaseResource.GetSqlDatabaseAdvisors();

                await foreach (var advisor in advisorCollection.GetAllAsync())
                {
                    // Only interested in the CreateIndex advisor
                    if (advisor.Data.Name == "CreateIndex" && advisor.Data.RecommendedActions != null)
                    {
                        foreach (var action in advisor.Data.RecommendedActions)
                        {
                            // Map the recommended action to your SqlIndexRecommendation model as needed
                            recommendations.Add(new Models.Sql.SqlIndexRecommendation
                            {
                                Name = action.Name ?? string.Empty,
                                Description = action.Details?.ToString() ?? string.Empty,
                                Impact = 0, // Set to 0 or map from action if available
                                TableName = string.Empty // Set to empty or map from action.Details if available
                            });
                        }
                    }
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advisor recommendations for database {Database} on server {Server}",
                    database, server);
                throw new SqlServiceException($"Error retrieving advisor recommendations: {ex.Message}", ex);
            }
        }
        catch (Exception ex) when (ex is not SqlServiceException)
        {
            var message = $"Error getting index recommendations for database '{database}' on server '{server}'";
            _logger.LogError(ex, message);
            throw new SqlServiceException(message, ex);
        }
    }

    public Task<List<string>> ListServers(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        // TODO: Implement actual logic
        return Task.FromResult(new List<string>());
    }

    public Task<List<string>> ListDatabases(string server, string resourceGroup, string subscription, string? tenant = null, AuthMethod? authMethod = null, RetryPolicyOptions? retryPolicy = null)
    {
        // TODO: Implement actual logic
        return Task.FromResult(new List<string>());
    }
}

public class SqlServiceException : Exception
{
    public SqlServiceException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public class SqlResourceNotFoundException : SqlServiceException
{
    public SqlResourceNotFoundException(string message)
        : base(message)
    {
    }
}
