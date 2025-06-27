using System.Threading.Tasks;
using Azure.ResourceManager.Resources;
using AzureMcp.Models.ResourceGroup;
using AzureMcp.Options;
using AzureMcp.Services.Azure.Arc;
using AzureMcp.Services.Azure.ResourceGroup;
using AzureMcp.Services.Azure.Subscription;
using AzureMcp.Services.Caching;
using AzureMcp.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

public class ArcServiceTests
{
    // Static variable shared by all tests in this class
    //private static string aksClusterName = "testaks" + Guid.NewGuid().ToString("N").Substring(0, 8);

    private static readonly string subscriptionId = "15c06b1b-01d6-407b-bb21-740b8617dea3";
    private static readonly string resourceGroupName = "rg-rosalinswain-7218";
    private static readonly string region = "eastus2";
    private static readonly string tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";

    private static readonly IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly ICacheService cacheService = new CacheService(memoryCache);
    private static readonly ITenantService tenantService = new AzureMcp.Services.Azure.Tenant.TenantService(cacheService);
    private static readonly ISubscriptionService subscriptionService = new SubscriptionService(cacheService, tenantService);
    private static readonly IResourceGroupService resourceGroupService = new ResourceGroupService(cacheService, subscriptionService);

    [Fact]
    public async Task CanCreateAksCluster()
    {
        /* // Replace these with your actual Azure values
         string subscriptionId = "15c06b1b-01d6-407b-bb21-740b8617dea3";
         string resourceGroupName = "rg-rosalinswain-7218";
         // string aksClusterName = "testaks" + Guid.NewGuid().ToString("N").Substring(0, 8);
         string aksClusterName = "testaks_existing"; // Use an existing AKS cluster name for testing
         string region = "eastus";
         string tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";

         // Use production implementations
         IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
         ICacheService cacheService = new CacheService(memoryCache);
         ITenantService tenantService = new AzureMcp.Services.Azure.Tenant.TenantService(cacheService);
         ISubscriptionService subscriptionService = new SubscriptionService(cacheService, tenantService);
         IResourceGroupService resourceGroupService = new ResourceGroupService(cacheService, subscriptionService);*/

        string aksClusterName = "testaks-existing";

        // Call the real method (ensure you are authenticated with Azure)
        IArcService arcService = new ArcService(subscriptionService, tenantService, cacheService, resourceGroupService);

        var result = await arcService.CreateAksClusterAsync(
            resourceGroupService, subscriptionId, resourceGroupName, aksClusterName, region, tenantId);
        Assert.Contains("/managedClusters/", result);
    }

    [Fact]
    public async Task CanConnectAksClusterToAzureArc()
    {
        /* // Replace these with your actual Azure values and an existing AKS cluster
         string resourceGroupName = "rg-rosalinswain-7218";
         string aksClusterName = "testaks-existing"; // Use an existing AKS cluster name
         string location = "eastus2euap";*/
        string aksClusterName = "testaks-existing";
        // Call the real method (ensure you are authenticated with Azure)
        IArcService arcService = new ArcService(subscriptionService, tenantService, cacheService, resourceGroupService);

        bool result = await arcService.ConnectAksToArcAsync(
            resourceGroupName,
            aksClusterName,
            region,
            msg => Console.WriteLine($"[Arc Connect] {msg}")
        );

        Assert.True(result, "Failed to connect AKS cluster to Azure Arc.");
    }

    [Fact]
    public async Task CanInstallAioPlatformExtension()
    {
        // Use the same values as your other tests
        string arcClusterName = "testaks-existing"; // Use your real connected cluster name
        // Call the real method (ensure you are authenticated with Azure)
        IArcService arcService = new ArcService(subscriptionService, tenantService, cacheService, resourceGroupService);

        bool result = await arcService.InstallAioPlatformExtensionAsync(
            resourceGroupName,
            arcClusterName,
            msg => Console.WriteLine($"[AIO Extension] {msg}")
        );

        Assert.True(result, "Failed to install AIO Platform extension.");
    }

    [Fact]
    public async Task CanInstallSecretSyncServiceExtension()
    {
        string arcClusterName = "testaks-existing"; // Use your real connected cluster name

        // Call the real method (ensure you are authenticated with Azure)
        IArcService arcService = new ArcService(subscriptionService, tenantService, cacheService, resourceGroupService);

        bool result = await arcService.InstallSecretSyncServiceExtensionAsync(
            resourceGroupName,
            arcClusterName,
            msg => Console.WriteLine($"[SSE Extension] {msg}")
        );

        Assert.True(result, "Failed to install Secret Sync Service (SSE) extension.");
    }

    [Fact]
    public async Task CanInstallAcsaExtension()
    {
        string arcClusterName = "testaks-existing"; // Use your real connected cluster name
                                                    // Call the real method (ensure you are authenticated with Azure)
        IArcService arcService = new ArcService(subscriptionService, tenantService, cacheService, resourceGroupService);

        bool result = await arcService.InstallAcsaExtensionAsync(
            resourceGroupName,
            arcClusterName,
            msg => Console.WriteLine($"[ACSA Extension] {msg}")
        );

        Assert.True(result, "Failed to install ACSA extension.");
    }
}
/*public class TestResourceGroupService : IResourceGroupService
{
    public Task<List<ResourceGroupInfo>> GetResourceGroups(string subscriptionId, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
        => Task.FromResult(new List<ResourceGroupInfo>());

    public Task<ResourceGroupInfo?> GetResourceGroup(string subscriptionId, string resourceGroupName, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
        => Task.FromResult<ResourceGroupInfo?>(new ResourceGroupInfo(resourceGroupName, "fake-id", "eastus"));

     public Task<ResourceGroupResource?> GetResourceGroupResource(string subscriptionId, string resourceGroupName, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
     {
         // Only return a fake for the expected test values
         if (subscriptionId == "15c06b1b-01d6-407b-bb21-740b8617dea3" && resourceGroupName == "rg-rosalinswain-7218")
         {
             // Create a fake ResourceGroupData
             var data = new ResourceGroupData("eastus");

             // Create a ResourceIdentifier for the group
             var id = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
             // Create a ResourceGroupResource using reflection (since constructor is internal)
             var resourceGroup = (ResourceGroupResource)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ResourceGroupResource));
             typeof(ResourceGroupResource).GetProperty("Id")?.SetValue(resourceGroup, id);
             typeof(ResourceGroupResource).GetProperty("Data")?.SetValue(resourceGroup, data);
             return Task.FromResult<ResourceGroupResource?>(resourceGroup);
         }
         return Task.FromResult<ResourceGroupResource?>(null);
     }
}*/