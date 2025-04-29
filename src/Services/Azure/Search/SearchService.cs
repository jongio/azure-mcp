// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.Search;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using AzureMcp.Arguments;
using AzureMcp.Services.Interfaces;

namespace AzureMcp.Services.Azure.Search;

public sealed class SearchService(ISubscriptionService subscriptionService, ICacheService cacheService) : BaseAzureService, ISearchService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private const string SEARCH_SERVICES_CACHE_KEY = "search_services";
    private static readonly TimeSpan CACHE_DURATION_SERVICES = TimeSpan.FromHours(1);

    public async Task<List<string>> ListServices(
        string subscription,
        string? tenantId = null,
        RetryPolicyArguments? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var cacheKey = string.IsNullOrEmpty(tenantId)
            ? $"{SEARCH_SERVICES_CACHE_KEY}_{subscription}"
            : $"{SEARCH_SERVICES_CACHE_KEY}_{subscription}_{tenantId}";

        var cachedServices = await _cacheService.GetAsync<List<string>>(cacheKey, CACHE_DURATION_SERVICES);
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

            await _cacheService.SetAsync(cacheKey, services, CACHE_DURATION_SERVICES);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Search services: {ex.Message}", ex);
        }

        return services;
    }

    public async Task<List<string>> ListIndexes(
        string serviceName,
        RetryPolicyArguments? retryPolicy = null)
    {
        ValidateRequiredParameters(serviceName);

        var indexes = new List<string>();

        try
        {
            var credential = await GetCredential();

            var clientOptions = AddDefaultPolicies(new SearchClientOptions());
            ConfigureRetryPolicy(clientOptions, retryPolicy);

            var endpoint = new Uri($"https://{serviceName}.search.windows.net");
            var searchClient = new SearchIndexClient(endpoint, credential, clientOptions);

            await foreach (var indexName in searchClient.GetIndexNamesAsync())
            {
                indexes.Add(indexName);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Search indexes: {ex.Message}", ex);
        }

        return indexes;
    }

    public async Task<object> DescribeIndex(
        string serviceName,
        string indexName,
        RetryPolicyArguments? retryPolicy = null)
    {
        ValidateRequiredParameters(serviceName, indexName);

        try
        {
            var credential = await GetCredential();

            var clientOptions = AddDefaultPolicies(new SearchClientOptions());
            ConfigureRetryPolicy(clientOptions, retryPolicy);

            var endpoint = new Uri($"https://{serviceName}.search.windows.net");
            var searchClient = new SearchIndexClient(endpoint, credential, clientOptions);

            var index = await searchClient.GetIndexAsync(indexName);

            return index.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Search index details: {ex.Message}", ex);
        }
    }

    private static void ConfigureRetryPolicy(SearchClientOptions options, RetryPolicyArguments? retryPolicy)
    {
        if (retryPolicy != null)
        {
            options.Retry.MaxRetries = retryPolicy.MaxRetries;
            options.Retry.Mode = retryPolicy.Mode;
            options.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.DelaySeconds);
            options.Retry.MaxDelay = TimeSpan.FromSeconds(retryPolicy.MaxDelaySeconds);
            options.Retry.NetworkTimeout = TimeSpan.FromSeconds(retryPolicy.NetworkTimeoutSeconds);
        }
    }
}