using System.Threading.Tasks;
using AzureMcp.Services.Azure.Arc;

namespace AzureMcp.Services.Interfaces
{
    public interface IArcService
    {
        Task<DeploymentResult> DeployAksEdgeEssentialClusterAsync(); // Removed outputFilePath parameter
        Task<bool> DeployAksClusterToArcAsync(string resourceGroup, string clusterName, string location);
        //void ConnectAkseeClusterToAzureArc(string clusterName, string resourceGroupName, string location, string subscriptionId, string tenantId);
        string LoadDeploymentSteps();
        Task<bool> RemoveAksEdgeAsync();
        Task<bool> ConnectClusterToArcAsync(string clusterName, string resourceGroupName, string location, string subscriptionId, string tenantId);
        // Task<bool> ExecuteScriptAsync(string scriptPath1, string scriptPath2); // Added method for script execution abstraction
    }
}
