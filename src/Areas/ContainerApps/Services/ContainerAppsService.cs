// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.AppContainers;
using AzureMcp.Areas.ContainerApps.Models;
using AzureMcp.Options;
using AzureMcp.Services.Azure;
using AzureMcp.Services.Azure.Subscription;
using AzureMcp.Services.Azure.Tenant;

namespace AzureMcp.Areas.ContainerApps.Services;

public class ContainerAppsService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IContainerAppsService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<List<ContainerApp>> ListApps(
        string subscription,
        string? resourceGroupName = null,
        string? environmentName = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var apps = new List<ContainerApp>();

        try
        {
            if (!string.IsNullOrEmpty(resourceGroupName))
            {
                // List apps in specific resource group
                var resourceGroup = await subscriptionResource.GetResourceGroupAsync(resourceGroupName);
                if (resourceGroup?.Value != null)
                {
                    await foreach (var appResource in subscriptionResource.GetContainerAppsAsync())
                    {
                        // Filter by resource group
                        if (appResource.Id.ResourceGroupName?.Equals(resourceGroupName, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            var app = ConvertToContainerApp(appResource);
                            if (string.IsNullOrEmpty(environmentName) ||
                                IsAppInEnvironment(app, environmentName))
                            {
                                apps.Add(app);
                            }
                        }
                    }
                }
            }
            else
            {
                // List all apps in subscription
                await foreach (var appResource in subscriptionResource.GetContainerAppsAsync())
                {
                    var app = ConvertToContainerApp(appResource);
                    if (string.IsNullOrEmpty(environmentName) ||
                        IsAppInEnvironment(app, environmentName))
                    {
                        apps.Add(app);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing container apps: {ex.Message}", ex);
        }

        return apps;
    }

    private static ContainerApp ConvertToContainerApp(ContainerAppResource appResource)
    {
        var data = appResource.Data;

        return new ContainerApp
        {
            Name = data.Name,
            Id = data.Id?.ToString(),
            Type = data.ResourceType.ToString(),
            Location = data.Location.ToString(),
            ResourceGroup = appResource.Id.ResourceGroupName,
            SubscriptionId = appResource.Id.SubscriptionId?.ToString(),
            ManagedEnvironmentId = data.EnvironmentId?.ToString(),
            ProvisioningState = data.ProvisioningState?.ToString(),
            Tags = data.Tags?.ToDictionary(t => t.Key, t => t.Value?.ToString() ?? string.Empty)
        };
    }

    private static bool IsAppInEnvironment(ContainerApp app, string environmentName)
    {
        if (app.ManagedEnvironmentId == null)
            return false;

        // Check if the environment ID contains the environment name
        return app.ManagedEnvironmentId.Contains(environmentName, StringComparison.OrdinalIgnoreCase) ||
               app.ManagedEnvironmentId.EndsWith($"/{environmentName}", StringComparison.OrdinalIgnoreCase);
    }
}
