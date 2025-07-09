using System.Diagnostics;
using System.Threading.Tasks;
using AzureMcp.Services.Azure.Arc;

namespace AzureMcp.Services.Interfaces
{
    public interface IArcService
    {
        Task<DeploymentResult> DeployAksEdgeEssentialClusterAsync(); // Removed outputFilePath parameter
        Task<bool> DeployAksClusterToArcAsync(string resourceGroup, string clusterName, string location);
        //void ConnectAkseeClusterToAzureArc(string clusterName, string resourceGroupName, string location, string subscriptionId, string tenantId);
        string LoadResourceFiles(string resourceName);
        Process StartProcess(string scriptPath, ProcessStartInfo processInfo);
        Task<bool> RemoveAksEdgeAsync(string userProvidedPath);
        Task<DeploymentResult> ValidatePrerequisitesForAksEdgeClusterAsync();
        Task<bool> ConnectClusterToArcAsync(string clusterName, string resourceGroupName, string location, string subscriptionId, string tenantId);
        // Task<bool> ExecuteScriptAsync(string scriptPath1, string scriptPath2); // Added method for script execution abstraction
        Task<DeploymentResult> ValidateSystemRequirementsAndSetupHyperVAsync(string userProvidedPath);
        Task<DeploymentResult> ValidateAndInstallSwRequirementAsync(string userProvidedPath);
        Task<DeploymentResult> QuickDeployAksEdgeEssentialsAsync(string clusterName, string resourceGroupName, string subscriptionId, string tenantId, string location, string userProvidedPath);
    }
}
