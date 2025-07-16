using System.Diagnostics;
using System.Threading.Tasks;
using AzureMcp.Services.Azure.Arc;

namespace AzureMcp.Services.Interfaces
{
    public interface IArcService
    {
        string LoadResourceFiles(string resourceName);
        Process StartProcess(string scriptPath, ProcessStartInfo processInfo);
        Task<bool> RemoveAksEdgeAsync(string userProvidedPath);
        Task<DeploymentResult> ValidatePrerequisitesForAksEdgeClusterAsync();
        Task<bool> OnboardClusterToArcAsync(string clusterName, string resourceGroupName, string location, string subscriptionId, string tenantId, string kubeConfigPath);
        // Task<bool> ExecuteScriptAsync(string scriptPath1, string scriptPath2); // Added method for script execution abstraction
        Task<DeploymentResult> ValidateSystemRequirementsAndSetupHyperVAsync(string userProvidedPath);
        Task<DeploymentResult> ValidateAndInstallSwRequirementAsync(string userProvidedPath);
        Task<DeploymentResult> QuickDeployAksEdgeEssentialsAsync(string clusterName, string resourceGroupName, string subscriptionId, string tenantId, string location, string userProvidedPath);
    }
}
