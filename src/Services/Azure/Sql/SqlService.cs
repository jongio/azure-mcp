// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Sql;
using AzureMcp.Models.Sql;
using AzureMcp.Options;
using AzureMcp.Services.Azure.Sql.Exceptions;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Services.Azure.Sql;

public sealed class SqlService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService) : BaseAzureService(tenantService), ISqlService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string CACHE_GROUP = "sql";
    private const string SQL_SERVERS_CACHE_KEY = "servers"; // Keeping the constant name in caps per C# convention
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

    private new void ValidateRequiredParameters(params string[] parameters)
    {
        foreach (var param in parameters)
        {
            ArgumentException.ThrowIfNullOrEmpty(param, param);
        }
    }

    public async Task<List<SqlIndexRecommendation>> GetIndexRecommendationsAsync(
        string database,
        string server,
        string? tableName,
        int? minImpact,
        string subscription,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, server, database);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

        // Note: Return empty list for now since we need to implement the Sql Database Advisor APIs
        // We'll need to get the resource group name to properly implement this
        await Task.Delay(1); // Placeholder for async operation
        return new List<SqlIndexRecommendation>();
    }    public async Task<List<string>> ListServers(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{SQL_SERVERS_CACHE_KEY}_{subscription}"
            : $"{SQL_SERVERS_CACHE_KEY}_{subscription}_{tenant}";

        var cachedServers = await _cacheService.GetAsync<List<string>>(CACHE_GROUP, cacheKey, CACHE_DURATION);
        if (cachedServers != null)
        {
            return cachedServers;
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var servers = new List<string>();

        try
        {
            await foreach (var sqlServer in subscriptionResource.GetSqlServersAsync())
            {
                if (sqlServer?.Data?.Name != null)
                {
                    servers.Add(sqlServer.Data.Name);
                }
            }

            await _cacheService.SetAsync(CACHE_GROUP, cacheKey, servers, CACHE_DURATION);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Sql servers: {ex.Message}", ex);
        }

        return servers;
    }

    public async Task<List<string>> ListDatabases(
        string subscription,
        string resourceGroup,
        string server,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup, server);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
        if (resourceGroupResource?.Value == null)
        {
            throw new Exception($"Resource group '{resourceGroup}' not found in subscription '{subscription}'");
        }

        var sqlServer = await resourceGroupResource.Value.GetSqlServerAsync(server);
        if (sqlServer?.Value == null)
        {
            throw new Exception($"SQL server '{server}' not found in resource group '{resourceGroup}'");
        }

        var databases = new List<string>();
        try
        {
            await foreach (var database in sqlServer.Value.GetSqlDatabases().GetAllAsync())
            {
                if (database?.Data?.Name != null)
                {
                    databases.Add(database.Data.Name);
                }
            }

            return databases;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving SQL databases: {ex.Message}", ex);
        }
    }
}
