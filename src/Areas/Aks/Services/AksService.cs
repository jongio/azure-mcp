// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.ContainerService;
using AzureMcp.Options;
using AzureMcp.Services.Azure;
using AzureMcp.Services.Azure.Subscription;
using AzureMcp.Services.Azure.Tenant;
using AzureMcp.Services.Caching;

namespace AzureMcp.Areas.Aks.Services;

public sealed class AksService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService) : BaseAzureService(tenantService), IAksService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    private const string CacheGroup = "aks";
    private const string AksClustersCacheKey = "clusters";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<string>> ListClusters(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        // Create cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{AksClustersCacheKey}_{subscription}"
            : $"{AksClustersCacheKey}_{subscription}_{tenant}";

        // Try to get from cache first
        var cachedClusters = await _cacheService.GetAsync<List<string>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedClusters != null)
        {
            return cachedClusters;
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var clusters = new List<string>();

        try
        {
            await foreach (var cluster in subscriptionResource.GetContainerServiceManagedClustersAsync())
            {
                if (cluster?.Data?.Name != null)
                {
                    clusters.Add(cluster.Data.Name);
                }
            }

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, clusters, s_cacheDuration);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving AKS clusters: {ex.Message}", ex);
        }

        return clusters;
    }
}
