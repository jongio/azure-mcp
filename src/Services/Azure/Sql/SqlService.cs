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
        string? tableName,
        int? minImpact,
        string subscription,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, server, database);

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
            var subscriptionResource = armClient.GetDefaultSubscription();

            // Find the server resource
            var serverResponse = await subscriptionResource.GetSqlServerAsync(server);

            if (serverResponse?.Value == null)
            {
                throw new SqlResourceNotFoundException($"SQL Server '{server}' not found in subscription '{subscription}'");
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
                // Get all advisors for the database
                await foreach (var advisor in databaseResource.GetAdvisors())
                {
                    if (advisor?.Data == null || advisor.Data.AdvisorType != "CreateIndex")
                    {
                        continue;
                    }

                    _logger.LogInformation("Processing advisor {AdvisorName} of type {AdvisorType}",
                        advisor.Data.Name, advisor.Data.AdvisorType);

                    try
                    {
                        await foreach (var recommendation in advisor.GetRecommendedActions())
                        {
                            if (recommendation?.Data == null)
                            {
                                continue;
                            }

                            var details = recommendation.GetDetails();
                            if (details == null)
                            {
                                _logger.LogWarning("Unable to get details for recommendation {RecommendationName}",
                                    recommendation.Data.Name);
                                continue;
                            }

                            // Apply filters
                            if (minImpact.HasValue && recommendation.Data.EstimatedImpact < minImpact.Value / 100.0)
                            {
                                continue;
                            }

                            var recommendationTableName = details.TableName;
                            if (!string.IsNullOrEmpty(tableName) &&
                                !recommendationTableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            recommendations.Add(new Models.Sql.SqlIndexRecommendation
                            {
                                Name = recommendation.Data.Name ?? string.Empty,
                                Description = recommendation.Data.Details ?? string.Empty,
                                Impact = (int)(recommendation.Data.EstimatedImpact * 100), // Convert to percentage
                                TableName = recommendationTableName
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing recommendations for advisor {AdvisorName}. Continuing with next advisor.",
                            advisor.Data.Name);
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
