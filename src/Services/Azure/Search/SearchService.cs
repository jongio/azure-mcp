// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.Search;
using AzureMcp.Arguments;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Services.Azure.Search;

public sealed class SearchService(ISubscriptionService subscriptionService, ICacheService cacheService) : BaseAzureService, ISearchService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string SEARCH_SERVICES_CACHE_KEY = "search_services";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

    public async Task<List<string>> ListServices(
        string subscription,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var cacheKey = string.IsNullOrEmpty(tenantId)
            ? $"{SEARCH_SERVICES_CACHE_KEY}_{subscription}"
            : $"{SEARCH_SERVICES_CACHE_KEY}_{subscription}_{tenantId}";

        var cachedServices = await _cacheService.GetAsync<List<string>>(cacheKey, CACHE_DURATION);
        if (cachedServices != null)
        {
            return cachedServices;
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId, retryPolicy);
        var services = new List<string>();
        try
        {
            await foreach (var service in subscriptionResource.GetSearchServicesAsync())
            {
                if (service?.Data?.Name != null)
                {
                    services.Add(service.Data.Name);
                }
            }

            await _cacheService.SetAsync(cacheKey, services, CACHE_DURATION);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Search services: {ex.Message}", ex);
        }

        return services;
    }
}
