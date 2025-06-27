using Azure.Identity;
using Azure.ResourceManager.ContainerService;
using Azure.ResourceManager.ContainerService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using System.Threading.Tasks;
using Azure.ResourceManager.Models;

public static class AksHelper
{
    public static async Task<bool> CreateAksClusterAsync(
        string subscriptionId,
        string resourceGroupName,
        string aksClusterName,
        string location)
    {
        var credential = new DefaultAzureCredential();
        var armClient = new ArmClient(credential, subscriptionId);

        // Get the resource group
        var resourceGroup = await armClient.GetResourceGroupResource(
            ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName))
            .GetAsync();

        var aksData = new ContainerServiceManagedClusterData(location)
        {
            DnsPrefix = aksClusterName,
            AgentPoolProfiles =
            {
                new ManagedClusterAgentPoolProfile("nodepool1")
                {
                    Count = 2,
                    VmSize = "Standard_DS2_v2",
                    OSType = "Linux",
                    Mode = AgentPoolMode.System
                }
            },
            Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned)
        };

        var aksCollection = resourceGroup.Value.GetContainerServiceManagedClusters();
        await aksCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, aksClusterName, aksData);

        return true;
    }
}